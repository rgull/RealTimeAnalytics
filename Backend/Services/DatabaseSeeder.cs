using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Services
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new();

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if sensors already exist
            if (await _context.Sensors.AnyAsync())
            {
                return; // Database already seeded
            }

            // Create sample sensors
            var sensors = new List<Sensor>
            {
                new Sensor 
                { 
                    Name = "Temperature Sensor 1", 
                    Type = "Temperature", 
                    Location = "Room A", 
                    Description = "Main temperature sensor for Room A", 
                    Unit = "°C",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Humidity Sensor 1", 
                    Type = "Humidity", 
                    Location = "Room A", 
                    Description = "Main humidity sensor for Room A", 
                    Unit = "%",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Pressure Sensor 1", 
                    Type = "Pressure", 
                    Location = "Room A", 
                    Description = "Atmospheric pressure sensor", 
                    Unit = "hPa",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Light Sensor 1", 
                    Type = "Light", 
                    Location = "Room A", 
                    Description = "Ambient light sensor", 
                    Unit = "lux",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Motion Sensor 1", 
                    Type = "Motion", 
                    Location = "Room A", 
                    Description = "Motion detection sensor", 
                    Unit = "count",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Sound Sensor 1", 
                    Type = "Sound", 
                    Location = "Room A", 
                    Description = "Sound level sensor", 
                    Unit = "dB",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Temperature Sensor 2", 
                    Type = "Temperature", 
                    Location = "Room B", 
                    Description = "Secondary temperature sensor for Room B", 
                    Unit = "°C",
                    IsActive = true
                },
                new Sensor 
                { 
                    Name = "Humidity Sensor 2", 
                    Type = "Humidity", 
                    Location = "Room B", 
                    Description = "Secondary humidity sensor for Room B", 
                    Unit = "%",
                    IsActive = true
                }
            };

            _context.Sensors.AddRange(sensors);
            await _context.SaveChangesAsync();

            // Generate sensor readings for the last 24 hours
            sensors = _context.Sensors.ToList();
            await GenerateSensorReadingsAsync(sensors);

            // Generate sensor statistics
            await GenerateSensorStatisticsAsync(sensors);
        }

        private async Task GenerateSensorReadingsAsync(List<Sensor> sensors)
        {
            var readings = new List<SensorReading>();
            var startTime = DateTime.UtcNow.AddHours(-24);
            var endTime = DateTime.UtcNow;

            foreach (var sensor in sensors)
            {
                // Generate readings every 5 minutes for the last 24 hours (288 readings)
                for (int i = 0; i < 288; i++)
                {
                    var timestamp = startTime.AddMinutes(i * 5);
                    var value = GenerateRealisticValue(sensor.Type, timestamp);
                    
                    readings.Add(new SensorReading
                    {
                        SensorId = sensor.Id,
                        Value = value,
                        Unit = sensor.Unit,
                        Timestamp = timestamp,
                        Metadata = GenerateMetadata(sensor.Type, value)
                    });
                }
            }

            _context.SensorReadings.AddRange(readings);
            await _context.SaveChangesAsync();

            // Update sensor last reading times
            foreach (var sensor in sensors)
            {
                sensor.LastReadingAt = endTime;
            }
            await _context.SaveChangesAsync();
        }

        private async Task GenerateSensorStatisticsAsync(List<Sensor> sensors)
        {
            var statistics = new List<SensorStatistic>();

            foreach (var sensor in sensors)
            {
                var sensorReadings = await _context.SensorReadings
                    .Where(r => r.SensorId == sensor.Id)
                    .ToListAsync();

                if (sensorReadings.Any())
                {
                    var values = sensorReadings.Select(r => r.Value).ToList();
                    var min = values.Min();
                    var max = values.Max();
                    var average = values.Average();
                    var count = values.Count;
                    var variance = values.Select(v => Math.Pow(v - average, 2)).Average();
                    var standardDeviation = Math.Sqrt(variance);

                    statistics.Add(new SensorStatistic
                    {
                        SensorId = sensor.Id,
                        MinValue = min,
                        MaxValue = max,
                        AverageValue = average,
                        ReadingCount = count,
                        CreatedAt = DateTime.UtcNow,
                        
                    });
                }
            }

            _context.SensorStatistics.AddRange(statistics);
            await _context.SaveChangesAsync();
        }

        private double GenerateRealisticValue(string sensorType, DateTime timestamp)
        {
            // Add some time-based patterns for more realistic data
            var hour = timestamp.Hour;
            var dayOfWeek = (int)timestamp.DayOfWeek;
            
            return sensorType switch
            {
                "Temperature" => GenerateTemperatureValue(hour, dayOfWeek),
                "Humidity" => GenerateHumidityValue(hour, dayOfWeek),
                "Pressure" => GeneratePressureValue(hour, dayOfWeek),
                "Light" => GenerateLightValue(hour, dayOfWeek),
                "Motion" => GenerateMotionValue(hour, dayOfWeek),
                "Sound" => GenerateSoundValue(hour, dayOfWeek),
                _ => _random.NextDouble() * 100
            };
        }

        private double GenerateTemperatureValue(int hour, int dayOfWeek)
        {
            // Base temperature with daily and weekly patterns
            var baseTemp = 22.0;
            var dailyVariation = Math.Sin((hour - 6) * Math.PI / 12) * 8; // Peak at 2 PM
            var weeklyVariation = dayOfWeek > 5 ? 2 : 0; // Warmer on weekends
            var noise = (_random.NextDouble() - 0.5) * 4;
            
            return Math.Round(baseTemp + dailyVariation + weeklyVariation + noise, 1);
        }

        private double GenerateHumidityValue(int hour, int dayOfWeek)
        {
            // Humidity inversely related to temperature
            var baseHumidity = 50.0;
            var dailyVariation = Math.Sin((hour - 6) * Math.PI / 12) * -15; // Lower during day
            var noise = (_random.NextDouble() - 0.5) * 10;
            
            return Math.Round(Math.Max(20, Math.Min(90, baseHumidity + dailyVariation + noise)), 1);
        }

        private double GeneratePressureValue(int hour, int dayOfWeek)
        {
            // Atmospheric pressure with slight daily variation
            var basePressure = 1013.0;
            var dailyVariation = Math.Sin((hour - 6) * Math.PI / 12) * 2;
            var noise = (_random.NextDouble() - 0.5) * 5;
            
            return Math.Round(basePressure + dailyVariation + noise, 1);
        }

        private double GenerateLightValue(int hour, int dayOfWeek)
        {
            // Light intensity based on time of day
            var baseLight = 0.0;
            if (hour >= 6 && hour <= 18)
            {
                var sunAngle = Math.Sin((hour - 6) * Math.PI / 12);
                baseLight = sunAngle * 800 + 200; // Peak at noon
            }
            var noise = _random.NextDouble() * 50;
            
            return Math.Round(Math.Max(0, baseLight + noise), 1);
        }

        private double GenerateMotionValue(int hour, int dayOfWeek)
        {
            // Higher motion during business hours and weekdays
            var baseMotion = 0.0;
            if (hour >= 8 && hour <= 18)
            {
                baseMotion = dayOfWeek < 6 ? 2.0 : 1.0; // More activity on weekdays
            }
            var noise = _random.NextDouble() * 2;
            
            return Math.Round(Math.Max(0, baseMotion + noise), 1);
        }

        private double GenerateSoundValue(int hour, int dayOfWeek)
        {
            // Sound level based on time and day
            var baseSound = 25.0;
            var dailyVariation = hour >= 8 && hour <= 18 ? 15 : 0; // Louder during day
            var weeklyVariation = dayOfWeek < 6 ? 5 : 0; // Louder on weekdays
            var noise = (_random.NextDouble() - 0.5) * 10;
            
            return Math.Round(Math.Max(20, Math.Min(80, baseSound + dailyVariation + weeklyVariation + noise)), 1);
        }

        private string GenerateMetadata(string sensorType, double value)
        {
            var quality = _random.NextDouble() * 0.2 + 0.8; // 0.8 to 1.0
            var battery = _random.Next(85, 101);
            var signal = _random.Next(70, 101);
            
            return $"{{\"simulated\": true, \"quality\": {quality:F2}, \"battery\": {battery}, \"signal\": {signal}}}";
        }
    }
}
