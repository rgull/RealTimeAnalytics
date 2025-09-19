using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Hubs;
using RealTimeSensorTrack.Models;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Services
{
    public interface IAnomalyDetectionService
    {
        Task CheckForAnomaliesAsync(SensorReading reading);
        Task ProcessBatchAnomaliesAsync(IEnumerable<SensorReading> readings);
    }

    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnomalyDetectionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IInMemoryDataService _inMemoryService;
        private readonly double _anomalyThreshold;

        public AnomalyDetectionService(
            IServiceProvider serviceProvider,
            ILogger<AnomalyDetectionService> logger,
            IConfiguration configuration,
            IInMemoryDataService inMemoryService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _inMemoryService = inMemoryService;
            _anomalyThreshold = _configuration.GetValue<double>("SensorSettings:AnomalyThreshold", 2.0);
        }

        public async Task CheckForAnomaliesAsync(SensorReading reading)
        {
            try
            {
                var recentReadings = _inMemoryService.GetRecentReadings(reading.SensorId, 50).ToList();
                
                if (recentReadings.Count < 10) return; // Need enough data for anomaly detection

                var values = recentReadings.Select(r => r.Value).ToList();
                var mean = values.Average();
                var stdDev = CalculateStandardDeviation(values, mean);

                if (stdDev == 0) return; // No variation, can't detect anomalies

                var zScore = Math.Abs((reading.Value - mean) / stdDev);

                if (zScore > _anomalyThreshold)
                {
                    await CreateAlertAsync(reading, zScore, mean, stdDev);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking for anomalies in reading {reading.Id}");
            }
        }

        public async Task ProcessBatchAnomaliesAsync(IEnumerable<SensorReading> readings)
        {
            var tasks = readings.Select(CheckForAnomaliesAsync);
            await Task.WhenAll(tasks);
        }

        private double CalculateStandardDeviation(List<double> values, double mean)
        {
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            return Math.Sqrt(variance);
        }

        private async Task CreateAlertAsync(SensorReading reading, double zScore, double mean, double stdDev)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var severity = zScore switch
            {
                >= 4.0 => "Critical",
                >= 3.0 => "Error",
                >= 2.0 => "Warning",
                _ => "Info"
            };

            var alert = new Alert
            {
                SensorId = reading.SensorId,
                Message = $"Anomaly detected: Value {reading.Value:F2} deviates {zScore:F2} standard deviations from mean {mean:F2}",
                Severity = severity,
                ThresholdValue = mean + (_anomalyThreshold * stdDev),
                ActualValue = reading.Value,
                CreatedAt = DateTime.UtcNow
            };

            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            _logger.LogWarning($"Anomaly alert created for sensor {reading.SensorId}: {alert.Message}");

            // Send real-time alert via SignalR
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SensorHub>>();
            await hubContext.Clients.All.SendAsync("NewAlert", alert);
        }
    }
}
