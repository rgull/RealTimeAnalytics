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
        Task GenerateAndSendReadingsContinuouslyAsync(CancellationToken stoppingToken, int recordMultiplier = 1);
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
        private readonly Dictionary<int, DateTime> _lastAlertTime = new(); // Track last alert time per sensor
        
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
        public async Task StartAutoSensorReadingGeneratorAsync(CancellationToken stoppingToken,int recordMultiplier = 1)
        {
            _logger.LogInformation("Starting auto sensor reading generator...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_isRunning && _sensors.Any())
                    {
                        await GenerateAndSendReadingsContinuouslyAsync(stoppingToken, recordMultiplier);
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
        public async Task GenerateAndSendReadingsContinuouslyAsync(CancellationToken stoppingToken, int recordMultiplier = 1)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inMemoryService = scope.ServiceProvider.GetRequiredService<IInMemoryDataService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SensorHub>>();

            var readings = new List<SensorReading>();
            if (_sensors == null || !_sensors.Any())
            {
                await InitializeSensorsAsync(); // Ensure sensors are loaded
            }

            // NEW: Adjusted number of readings to generate using recordMultiplier
            var readingsToGenerate = Math.Min(_batchSize * recordMultiplier, _simulationRate);

            for (int i = 0; i < readingsToGenerate && !stoppingToken.IsCancellationRequested; i++)
            {
                var sensor = _sensors[_random.Next(_sensors.Count)];
                var reading = GenerateSensorReading(sensor);

                readings.Add(reading);
                inMemoryService.AddReading(reading);

                if (i < readingsToGenerate - 1)
                {
                    await Task.Delay(_updateIntervalMs / readingsToGenerate, stoppingToken);
                }
            }

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
                    new Sensor { 
                        Name = "Temperature Sensor 1", 
                        Type = "Temperature", 
                        Location = "Room A", 
                        Description = "Main temperature sensor", 
                        Unit = "°C",
                        WarningThreshold = 20.0,
                        CriticalThreshold = 25.0,
                        MinThreshold = 5.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Humidity Sensor 1", 
                        Type = "Humidity", 
                        Location = "Room A", 
                        Description = "Main humidity sensor", 
                        Unit = "%",
                        WarningThreshold = 50.0,
                        CriticalThreshold = 60.0,
                        MinThreshold = 10.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Pressure Sensor 1", 
                        Type = "Pressure", 
                        Location = "Room A", 
                        Description = "Atmospheric pressure sensor", 
                        Unit = "hPa",
                        WarningThreshold = 950.0,
                        CriticalThreshold = 960.0,
                        MinThreshold = 900.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Light Sensor 1", 
                        Type = "Light", 
                        Location = "Room A", 
                        Description = "Ambient light sensor", 
                        Unit = "lux",
                        WarningThreshold = 500.0,
                        CriticalThreshold = 600.0,
                        MinThreshold = 100.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Motion Sensor 1", 
                        Type = "Motion", 
                        Location = "Room A", 
                        Description = "Motion detection sensor", 
                        Unit = "count",
                        WarningThreshold = 5.0,
                        CriticalThreshold = 8.0,
                        MinThreshold = 0.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Sound Sensor 1", 
                        Type = "Sound", 
                        Location = "Room A", 
                        Description = "Sound level sensor", 
                        Unit = "dB",
                        WarningThreshold = 60.0,
                        CriticalThreshold = 70.0,
                        MinThreshold = 10.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Temperature Sensor 2", 
                        Type = "Temperature", 
                        Location = "Room B", 
                        Description = "Secondary temperature sensor", 
                        Unit = "°C",
                        WarningThreshold = 20.0,
                        CriticalThreshold = 25.0,
                        MinThreshold = 5.0,
                        AlertEnabled = true
                    },
                    new Sensor { 
                        Name = "Humidity Sensor 2", 
                        Type = "Humidity", 
                        Location = "Room B", 
                        Description = "Secondary humidity sensor", 
                        Unit = "%",
                        WarningThreshold = 50.0,
                        CriticalThreshold = 60.0,
                        MinThreshold = 10.0,
                        AlertEnabled = true
                    }
                };

                context.Sensors.AddRange(sensors);
                await context.SaveChangesAsync();
            }

            // Update existing sensors with new thresholds
            var existingSensors = context.Sensors.ToList();
            foreach (var sensor in existingSensors)
            {
                // Update thresholds based on sensor type
                switch (sensor.Type)
                {
                    case "Temperature":
                        sensor.WarningThreshold = 20.0;
                        sensor.CriticalThreshold = 25.0;
                        sensor.MinThreshold = 5.0;
                        break;
                    case "Humidity":
                        sensor.WarningThreshold = 50.0;
                        sensor.CriticalThreshold = 60.0;
                        sensor.MinThreshold = 10.0;
                        break;
                    case "Pressure":
                        sensor.WarningThreshold = 950.0;
                        sensor.CriticalThreshold = 960.0;
                        sensor.MinThreshold = 900.0;
                        break;
                    case "Light":
                        sensor.WarningThreshold = 500.0;
                        sensor.CriticalThreshold = 600.0;
                        sensor.MinThreshold = 100.0;
                        break;
                    case "Motion":
                        sensor.WarningThreshold = 5.0;
                        sensor.CriticalThreshold = 8.0;
                        sensor.MinThreshold = 0.0;
                        break;
                    case "Sound":
                        sensor.WarningThreshold = 60.0;
                        sensor.CriticalThreshold = 70.0;
                        sensor.MinThreshold = 10.0;
                        break;
                }
                sensor.AlertEnabled = true;
            }
            
            await context.SaveChangesAsync();
            _logger.LogInformation("Updated sensor thresholds for alert generation");

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
            double value;
            bool shouldTriggerAlert = _random.NextDouble() < 0.001; // 0.1% chance to trigger alert (every 1-2 minutes)
            
            // Check cooldown period (minimum 60 seconds between alerts for same sensor)
            bool canTriggerAlert = true;
            if (_lastAlertTime.ContainsKey(sensor.Id))
            {
                var timeSinceLastAlert = DateTime.UtcNow - _lastAlertTime[sensor.Id];
                canTriggerAlert = timeSinceLastAlert.TotalSeconds >= 60; // 60 second cooldown (1 minute)
                
            }

            if (shouldTriggerAlert && sensor.AlertEnabled && canTriggerAlert)
            {
                // Update last alert time
                _lastAlertTime[sensor.Id] = DateTime.UtcNow;
                
                // Generate values that will trigger alerts based on sensor type
                value = sensor.Type switch
                {
                    "Temperature" => _random.NextDouble() * 15 + 30, // 30-45°C (high temperature)
                    "Humidity" => _random.NextDouble() * 30 + 70, // 70-100% (high humidity)
                    "Pressure" => _random.NextDouble() * 30 + 970, // 970-1000 hPa (high pressure)
                    "Light" => _random.NextDouble() * 300 + 700, // 700-1000 lux (high light)
                    "Motion" => _random.NextDouble() * 5 + 10, // 10-15 count (high motion)
                    "Sound" => _random.NextDouble() * 30 + 80, // 80-110 dB (high sound)
                    _ => _random.NextDouble() * 30 + 60
                };
                
                _logger.LogInformation("🚨 Generating alert-triggering value for {SensorName}: {Value} (Type: {Type})", 
                    sensor.Name, value, sensor.Type);
            }
            else
            {
                // Generate normal values (original logic)
                value = sensor.Type switch
                {
                    "Temperature" => _random.NextDouble() * 40 + 10, // 10-50°C
                    "Humidity" => _random.NextDouble() * 100, // 0-100%
                    "Pressure" => _random.NextDouble() * 200 + 900, // 900-1100 hPa
                    "Light" => _random.NextDouble() * 1000, // 0-1000 lux
                    "Motion" => _random.NextDouble() * 10, // 0-10 count
                    "Sound" => _random.NextDouble() * 100, // 0-100 dB
                    _ => _random.NextDouble() * 100
                };
            }

            return new SensorReading
            {
                SensorId = sensor.Id,
                Value = Math.Round(value, 2),
                Unit = sensor.Unit ?? GetUnitForType(sensor.Type),
                Timestamp = DateTime.UtcNow,
                Metadata = $"{{\"simulated\": true, \"quality\": {_random.NextDouble():F2}, \"alert_triggered\": {shouldTriggerAlert.ToString().ToLower()}}}"
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
