using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<SensorStatistic> SensorStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SensorReading for performance
            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SensorId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.SensorId, e.Timestamp });
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // Configure Sensor
            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Alert
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SensorId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.IsResolved);
            });

            // Configure SensorStatistic
            modelBuilder.Entity<SensorStatistic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SensorId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => new { e.SensorId, e.Date }).IsUnique();
            });

            // Configure relationships
            modelBuilder.Entity<SensorReading>()
                .HasOne(sr => sr.Sensor)
                .WithMany(s => s.SensorReadings)
                .HasForeignKey(sr => sr.SensorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Sensor)
                .WithMany(s => s.Alerts)
                .HasForeignKey(a => a.SensorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SensorStatistic>()
                .HasOne(ss => ss.Sensor)
                .WithMany(s => s.SensorStatistics)
                .HasForeignKey(ss => ss.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
