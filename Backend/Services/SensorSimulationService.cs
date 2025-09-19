using Microsoft.AspNetCore.SignalR;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Hubs;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Services
{
    public interface ISensorSimulationService
    {
        Task StartSimulationAsync();
        Task StopSimulationAsync();
        bool IsRunning { get; }
    }

    public class SensorSimulationService : BackgroundService, ISensorSimulationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SensorSimulationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Random _random = new();
        private readonly string[] _sensorTypes = { "Temperature", "Humidity", "Pressure", "Light", "Motion", "Sound" };
        private readonly string[] _units = { "°C", "%", "hPa", "lux", "count", "dB" };
        
        private int _simulationRate;
        private List<Sensor> _sensors = new();
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;

        public SensorSimulationService(
            IServiceProvider serviceProvider,
            ILogger<SensorSimulationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _simulationRate = _configuration.GetValue<int>("SensorSettings:SimulationRatePerSecond", 1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeSensorsAsync();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateReadingsBatchAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in sensor simulation service");
                }
                
                await Task.Delay(1000, stoppingToken); // Wait 1 second between batches
            }
        }

        private async Task InitializeSensorsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create sample sensors if none exist
            if (!context.Sensors.Any())
            {
                var sensors = new List<Sensor>
                {
                    new Sensor { Name = "Temperature Sensor 1", Type = "Temperature", Location = "Room A", Description = "Main temperature sensor", Unit = "°C" },
                    new Sensor { Name = "Humidity Sensor 1", Type = "Humidity", Location = "Room A", Description = "Main humidity sensor", Unit = "%" },
                    new Sensor { Name = "Pressure Sensor 1", Type = "Pressure", Location = "Room A", Description = "Atmospheric pressure sensor", Unit = "hPa" },
                    new Sensor { Name = "Light Sensor 1", Type = "Light", Location = "Room A", Description = "Ambient light sensor", Unit = "lux" },
                    new Sensor { Name = "Motion Sensor 1", Type = "Motion", Location = "Room A", Description = "Motion detection sensor", Unit = "count" },
                    new Sensor { Name = "Sound Sensor 1", Type = "Sound", Location = "Room A", Description = "Sound level sensor", Unit = "dB" },
                    new Sensor { Name = "Temperature Sensor 2", Type = "Temperature", Location = "Room B", Description = "Secondary temperature sensor", Unit = "°C" },
                    new Sensor { Name = "Humidity Sensor 2", Type = "Humidity", Location = "Room B", Description = "Secondary humidity sensor", Unit = "%" }
                };

                context.Sensors.AddRange(sensors);
                await context.SaveChangesAsync();
            }

            _sensors = context.Sensors.Where(s => s.IsActive).ToList();
            _logger.LogInformation($"Initialized {_sensors.Count} sensors for simulation");
        }

        public Task StartSimulationAsync()
        {
            if (_isRunning) return Task.CompletedTask;

            _isRunning = true;
            _logger.LogInformation($"Starting sensor simulation at {_simulationRate} readings per second");
            return Task.CompletedTask;
        }

        private async Task GenerateReadingsBatchAsync(CancellationToken stoppingToken)
        {
            if (!_isRunning || !_sensors.Any()) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inMemoryService = scope.ServiceProvider.GetRequiredService<IInMemoryDataService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SensorHub>>();

            var readings = new List<SensorReading>();
            var readingsToGenerate = Math.Min(_simulationRate, 100); // Limit batch size

            for (int i = 0; i < readingsToGenerate && !stoppingToken.IsCancellationRequested; i++)
            {
                var sensor = _sensors[_random.Next(_sensors.Count)];
                var reading = GenerateSensorReading(sensor);
                
                readings.Add(reading);
                inMemoryService.AddReading(reading);

                // Small delay to spread readings across the second
                if (i < readingsToGenerate - 1)
                {
                    await Task.Delay(1000 / readingsToGenerate, stoppingToken);
                }
            }

            // Batch save to database
            if (readings.Any())
            {
                context.SensorReadings.AddRange(readings);
                await context.SaveChangesAsync(stoppingToken);

                // Update sensor last reading time
                foreach (var sensorId in readings.Select(r => r.SensorId).Distinct())
                {
                    var sensorEntity = context.Sensors.Find(sensorId);
                    if (sensorEntity != null)
                    {
                        sensorEntity.LastReadingAt = DateTime.UtcNow;
                    }
                }
                await context.SaveChangesAsync(stoppingToken);

                // Send real-time updates via SignalR
                await hubContext.Clients.All.SendAsync("NewReadings", readings.TakeLast(10).ToList());
            }
        }


        private SensorReading GenerateSensorReading(Sensor sensor)
        {
            var value = sensor.Type switch
            {
                "Temperature" => _random.NextDouble() * 40 + 10, // 10-50°C
                "Humidity" => _random.NextDouble() * 100, // 0-100%
                "Pressure" => _random.NextDouble() * 200 + 900, // 900-1100 hPa
                "Light" => _random.NextDouble() * 1000, // 0-1000 lux
                "Motion" => _random.NextDouble() * 10, // 0-10 count
                "Sound" => _random.NextDouble() * 100, // 0-100 dB
                _ => _random.NextDouble() * 100
            };

            return new SensorReading
            {
                SensorId = sensor.Id,
                Value = Math.Round(value, 2),
                Unit = sensor.Unit ?? GetUnitForType(sensor.Type),
                Timestamp = DateTime.UtcNow,
                Metadata = $"{{\"simulated\": true, \"quality\": {_random.NextDouble():F2}}}"
            };
        }

        private string GetUnitForType(string type)
        {
            return type switch
            {
                "Temperature" => "°C",
                "Humidity" => "%",
                "Pressure" => "hPa",
                "Light" => "lux",
                "Motion" => "count",
                "Sound" => "dB",
                _ => "unit"
            };
        }

        public async Task StopSimulationAsync()
        {
            _isRunning = false;
            _logger.LogInformation("Stopping sensor simulation");
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopSimulationAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
