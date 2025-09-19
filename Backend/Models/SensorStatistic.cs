using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealTimeSensorTrack.Models
{
    public class SensorStatistic
    {
        public long Id { get; set; }
        
        public int SensorId { get; set; }
        
        public DateTime Date { get; set; }
        
        public double MinValue { get; set; }
        
        public double MaxValue { get; set; }
        
        public double AverageValue { get; set; }
        
        public int ReadingCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; } = null!;
    }
}
