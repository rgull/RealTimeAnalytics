using System.Collections.Concurrent;
using System.Collections.Generic;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Services
{
    public interface IInMemoryDataService
    {
        void AddReading(SensorReading reading);
        IEnumerable<SensorReading> GetRecentReadings(int sensorId, int count = 100);
        IEnumerable<SensorReading> GetAllRecentReadings(int count = 1000);
        void ClearOldData(DateTime cutoffTime);
        int GetReadingCount();
        IEnumerable<SensorReading> GetReadingsByTimeRange(DateTime startTime, DateTime endTime);
    }

    public class InMemoryDataService : IInMemoryDataService
    {
        private readonly ConcurrentQueue<SensorReading> _readings = new();
        private readonly ConcurrentDictionary<int, ConcurrentQueue<SensorReading>> _sensorReadings = new();
        private readonly object _lockObject = new();
        private int _maxRecords;

        public InMemoryDataService(IConfiguration configuration)
        {
            _maxRecords = configuration.GetValue<int>("SensorSettings:MaxInMemoryRecords", 100000);
        }

        public void AddReading(SensorReading reading)
        {
            lock (_lockObject)
            {
                // Add to main queue
                _readings.Enqueue(reading);

                // Add to sensor-specific queue
                if (!_sensorReadings.ContainsKey(reading.SensorId))
                {
                    _sensorReadings[reading.SensorId] = new ConcurrentQueue<SensorReading>();
                }
                _sensorReadings[reading.SensorId].Enqueue(reading);

                // Maintain max records limit
                while (_readings.Count > _maxRecords)
                {
                    if (_readings.TryDequeue(out var oldReading))
                    {
                        // Also remove from sensor-specific queue if it exists
                        if (_sensorReadings.TryGetValue(oldReading.SensorId, out var sensorQueue))
                        {
                            // Create a new queue without the old reading
                            var newQueue = new ConcurrentQueue<SensorReading>();
                            var tempList = new List<SensorReading>();
                            
                            while (sensorQueue.TryDequeue(out var item))
                            {
                                if (item.Id != oldReading.Id)
                                {
                                    tempList.Add(item);
                                }
                            }
                            
                            foreach (var item in tempList)
                            {
                                newQueue.Enqueue(item);
                            }
                            
                            _sensorReadings[oldReading.SensorId] = newQueue;
                        }
                    }
                }
            }
        }

        public IEnumerable<SensorReading> GetRecentReadings(int sensorId, int count = 100)
        {
            if (_sensorReadings.TryGetValue(sensorId, out var sensorQueue))
            {
                return sensorQueue.Skip(Math.Max(0, sensorQueue.Count - count));
            }
            return Enumerable.Empty<SensorReading>();
        }

        public IEnumerable<SensorReading> GetAllRecentReadings(int count = 1000)
        {
            return _readings.Skip(Math.Max(0, _readings.Count - count));
        }

        public void ClearOldData(DateTime cutoffTime)
        {
            lock (_lockObject)
            {
                // Clear main queue
                var newMainQueue = new ConcurrentQueue<SensorReading>();
                var recentReadings = _readings.Where(r => r.Timestamp >= cutoffTime).ToList();
                
                foreach (var reading in recentReadings)
                {
                    newMainQueue.Enqueue(reading);
                }
                
                // Replace the main queue
                while (_readings.TryDequeue(out _)) { }
                foreach (var reading in recentReadings)
                {
                    _readings.Enqueue(reading);
                }

                // Clear sensor-specific queues
                foreach (var sensorId in _sensorReadings.Keys.ToList())
                {
                    var sensorQueue = _sensorReadings[sensorId];
                    var newSensorQueue = new ConcurrentQueue<SensorReading>();
                    var recentSensorReadings = sensorQueue.Where(r => r.Timestamp >= cutoffTime).ToList();
                    
                    foreach (var reading in recentSensorReadings)
                    {
                        newSensorQueue.Enqueue(reading);
                    }
                    
                    _sensorReadings[sensorId] = newSensorQueue;
                }
            }
        }

        public int GetReadingCount()
        {
            return _readings.Count;
        }

        public IEnumerable<SensorReading> GetReadingsByTimeRange(DateTime startTime, DateTime endTime)
        {
            return _readings.Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime);
        }
    }
}
