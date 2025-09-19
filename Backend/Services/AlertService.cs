using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Hubs;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Services
{
    public interface IAlertService
    {
        Task CheckSensorReadingForAlertsAsync(SensorReading reading);
        Task CreateAlertAsync(int sensorId, string message, string severity, double? thresholdValue = null, double? actualValue = null);
        Task ResolveAlertAsync(long alertId);
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
    }

    public class AlertService : IAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly ILogger<AlertService> _logger;

        public AlertService(
            ApplicationDbContext context,
            IHubContext<SensorHub> hubContext,
            ILogger<AlertService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task CheckSensorReadingForAlertsAsync(SensorReading reading)
        {
            try
            {
                var sensor = await _context.Sensors
                    .FirstOrDefaultAsync(s => s.Id == reading.SensorId);

                if (sensor == null || !sensor.AlertEnabled)
                    return;

                var alerts = new List<(string message, string severity, double? thresholdValue)>();

                // Check for high value alerts
                if (sensor.CriticalThreshold.HasValue && reading.Value >= sensor.CriticalThreshold.Value)
                {
                    alerts.Add((
                        $"{sensor.Name} critical threshold exceeded! Value: {reading.Value:F2}{sensor.Unit} (Threshold: {sensor.CriticalThreshold:F2}{sensor.Unit})",
                        "Critical",
                        sensor.CriticalThreshold
                    ));
                }
                else if (sensor.WarningThreshold.HasValue && reading.Value >= sensor.WarningThreshold.Value)
                {
                    alerts.Add((
                        $"{sensor.Name} warning threshold exceeded! Value: {reading.Value:F2}{sensor.Unit} (Threshold: {sensor.WarningThreshold:F2}{sensor.Unit})",
                        "Warning",
                        sensor.WarningThreshold
                    ));
                }

                // Check for low value alerts
                if (sensor.MinThreshold.HasValue && reading.Value <= sensor.MinThreshold.Value)
                {
                    alerts.Add((
                        $"{sensor.Name} minimum threshold exceeded! Value: {reading.Value:F2}{sensor.Unit} (Min Threshold: {sensor.MinThreshold:F2}{sensor.Unit})",
                        "Warning",
                        sensor.MinThreshold
                    ));
                }

                // Create alerts
                foreach (var (message, severity, thresholdValue) in alerts)
                {
                    await CreateAlertAsync(reading.SensorId, message, severity, thresholdValue, reading.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking sensor reading for alerts: SensorId={SensorId}, Value={Value}", 
                    reading.SensorId, reading.Value);
            }
        }

        public async Task CreateAlertAsync(int sensorId, string message, string severity, double? thresholdValue = null, double? actualValue = null)
        {
            try
            {
                var alert = new Alert
                {
                    SensorId = sensorId,
                    Message = message,
                    Severity = severity,
                    ThresholdValue = thresholdValue,
                    ActualValue = actualValue,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert created: {AlertId} - {Message}", alert.Id, message);

                // Send real-time notification via SignalR
                await _hubContext.Clients.All.SendAsync("NewAlert", alert);
                await _hubContext.Clients.Group($"sensor_{sensorId}").SendAsync("NewAlert", alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert: SensorId={SensorId}, Message={Message}", 
                    sensorId, message);
            }
        }

        public async Task ResolveAlertAsync(long alertId)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(alertId);
                if (alert != null && !alert.IsResolved)
                {
                    alert.IsResolved = true;
                    alert.ResolvedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Alert resolved: {AlertId}", alertId);

                    // Send real-time notification via SignalR
                    await _hubContext.Clients.All.SendAsync("AlertResolved", alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving alert: AlertId={AlertId}", alertId);
            }
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            return await _context.Alerts
                .Include(a => a.Sensor)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
