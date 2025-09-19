using System.ComponentModel.DataAnnotations;

namespace RealTimeSensorTrack.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string? Unit { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastReadingAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public virtual ICollection<SensorStatistic> SensorStatistics { get; set; } = new List<SensorStatistic>();
    }
}
