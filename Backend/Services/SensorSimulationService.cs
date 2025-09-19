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
        private int _batchSize;
        private int _updateIntervalMs;
        private List<Sensor> _sensors = new();
        private bool _isRunning = false;
        private Timer _simulationTimer;
        private readonly object _lockObject = new object();

        public bool IsRunning => _isRunning;

        public SensorSimulationService(
            IServiceProvider serviceProvider,
            ILogger<SensorSimulationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _simulationRate = _configuration.GetValue<int>("SensorSettings:SimulationRatePerSecond", 10);
            _batchSize = _configuration.GetValue<int>("SensorSettings:BatchSize", 5);
            _updateIntervalMs = _configuration.GetValue<int>("SensorSettings:UpdateIntervalMs", 1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeSensorsAsync();
            
            // Auto-start the simulation
            _isRunning = true;
            _logger.LogInformation("Auto-starting sensor simulation service");
            
            // Start the auto sensor reading generator
            await StartAutoSensorReadingGeneratorAsync(stoppingToken);
        }

        /// <summary>
        /// Auto sensor reading generator that continuously creates and sends readings via SignalR
        /// </summary>
        private async Task StartAutoSensorReadingGeneratorAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting auto sensor reading generator...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_isRunning && _sensors.Any())
                    {
                        await GenerateAndSendReadingsContinuouslyAsync(stoppingToken);
                    }
                    
                    await Task.Delay(_updateIntervalMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in auto sensor reading generator");
                    await Task.Delay(5000, stoppingToken); // Wait 5 seconds before retrying
                }
            }
        }

        /// <summary>
        /// Continuously generates sensor readings and sends them via SignalR
        /// </summary>
        private async Task GenerateAndSendReadingsContinuouslyAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inMemoryService = scope.ServiceProvider.GetRequiredService<IInMemoryDataService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SensorHub>>();

            var readings = new List<SensorReading>();
            var readingsToGenerate = Math.Min(_batchSize, _simulationRate);

            // Generate readings for this batch
            for (int i = 0; i < readingsToGenerate && !stoppingToken.IsCancellationRequested; i++)
            {
                var sensor = _sensors[_random.Next(_sensors.Count)];
                var reading = GenerateSensorReading(sensor);
                
                readings.Add(reading);
                inMemoryService.AddReading(reading);

                // Small delay to spread readings across the interval
                if (i < readingsToGenerate - 1)
                {
                    await Task.Delay(_updateIntervalMs / readingsToGenerate, stoppingToken);
                }
            }

            // Save to database and send via SignalR
            if (readings.Any())
            {
                await SaveReadingsToDatabaseAsync(context, readings, stoppingToken);
                await SendReadingsViaSignalRAsync(hubContext, readings);
            }
        }

        /// <summary>
        /// Saves readings to database and updates sensor last reading time
        /// </summary>
        private async Task SaveReadingsToDatabaseAsync(ApplicationDbContext context, List<SensorReading> readings, CancellationToken stoppingToken)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving readings to database");
            }
        }

        /// <summary>
        /// Sends readings to all connected clients via SignalR
        /// </summary>
        private async Task SendReadingsViaSignalRAsync(IHubContext<SensorHub> hubContext, List<SensorReading> readings)
        {
            try
            {
                // Send to all connected clients
                await hubContext.Clients.All.SendAsync("NewReadings", readings);
                
                // Also send individual readings to subscribed sensor groups
                foreach (var reading in readings)
                {
                    await hubContext.Clients.Group($"sensor_{reading.SensorId}").SendAsync("NewReading", reading);
                }

                _logger.LogDebug("Sent {Count} readings via SignalR", readings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending readings via SignalR");
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
            lock (_lockObject)
            {
                if (_isRunning) return Task.CompletedTask;

                _isRunning = true;
                _logger.LogInformation($"Starting auto sensor reading generator at {_simulationRate} readings per second with {_batchSize} batch size");
            }
            return Task.CompletedTask;
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
            lock (_lockObject)
            {
                _isRunning = false;
                _logger.LogInformation("Stopping auto sensor reading generator");
            }
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopSimulationAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
