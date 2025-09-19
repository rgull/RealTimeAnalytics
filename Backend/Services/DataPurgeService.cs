using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Services
{
    public interface IDataPurgeService
    {
        Task PurgeOldDataAsync();
    }

    public class DataPurgeService : BackgroundService, IDataPurgeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataPurgeService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _purgeInterval = TimeSpan.FromHours(1); // Run every hour
        private readonly int _dataRetentionHours;

        public DataPurgeService(
            IServiceProvider serviceProvider,
            ILogger<DataPurgeService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _dataRetentionHours = _configuration.GetValue<int>("SensorSettings:DataRetentionHours", 24);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PurgeOldDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during data purge operation");
                }

                await Task.Delay(_purgeInterval, stoppingToken);
            }
        }

        public async Task PurgeOldDataAsync()
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-_dataRetentionHours);
            _logger.LogInformation($"Starting data purge for records older than {cutoffTime:yyyy-MM-dd HH:mm:ss} UTC");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inMemoryService = scope.ServiceProvider.GetRequiredService<IInMemoryDataService>();

            try
            {
                // Purge in-memory data
                inMemoryService.ClearOldData(cutoffTime);
                _logger.LogInformation($"Cleared in-memory data older than {cutoffTime:yyyy-MM-dd HH:mm:ss} UTC");

                // Purge database records
                var oldReadings = await context.SensorReadings
                    .Where(sr => sr.Timestamp < cutoffTime)
                    .ToListAsync();

                if (oldReadings.Any())
                {
                    context.SensorReadings.RemoveRange(oldReadings);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Purged {oldReadings.Count} old sensor readings from database");
                }

                // Purge old alerts (keep for 7 days)
                var oldAlerts = await context.Alerts
                    .Where(a => a.CreatedAt < DateTime.UtcNow.AddDays(-7))
                    .ToListAsync();

                if (oldAlerts.Any())
                {
                    context.Alerts.RemoveRange(oldAlerts);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Purged {oldAlerts.Count} old alerts from database");
                }

                // Purge old statistics (keep for 30 days)
                var oldStatistics = await context.SensorStatistics
                    .Where(ss => ss.CreatedAt < DateTime.UtcNow.AddDays(-30))
                    .ToListAsync();

                if (oldStatistics.Any())
                {
                    context.SensorStatistics.RemoveRange(oldStatistics);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Purged {oldStatistics.Count} old sensor statistics from database");
                }

                _logger.LogInformation("Data purge completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database purge operation");
                throw;
            }
        }
    }
}
