using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealTimeSensorTrack.Models
{
    public class SensorReading
    {
        public long Id { get; set; }
        
        public int SensorId { get; set; }
        
        public double Value { get; set; }
        
        public string? Unit { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? Metadata { get; set; } // JSON string for additional data

        // Navigation property
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; } = null!;
    }
}
