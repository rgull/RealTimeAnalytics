using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealTimeSensorTrack.Models
{
    public class Alert
    {
        public long Id { get; set; }
        
        public int SensorId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
        
        public double? ThresholdValue { get; set; }
        
        public double? ActualValue { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsResolved { get; set; } = false;
        
        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; } = null!;
    }
}
