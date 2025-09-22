using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Models;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlertsController> _logger;
        private readonly IAlertService _alertService;

        public AlertsController(ApplicationDbContext context, ILogger<AlertsController> logger, IAlertService alertService)
        {
            _context = context;
            _logger = logger;
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<Alert>>> GetAlerts([FromQuery] AlertsFilter filter)
        {
            var query = _context.Alerts
                .Include(a => a.Sensor)
                .AsQueryable();

            // Apply filters
            if (filter.SensorId.HasValue)
            {
                query = query.Where(a => a.SensorId == filter.SensorId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Severity))
            {
                query = query.Where(a => a.Severity == filter.Severity);
            }

            if (filter.IsResolved.HasValue)
            {
                query = query.Where(a => a.IsResolved == filter.IsResolved.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var response = new PaginatedResponse<Alert>(
                alerts, 
                filter.Page, 
                filter.PageSize, 
                totalCount);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Alert>> GetAlert(long id)
        {
            var alert = await _context.Alerts
                .Include(a => a.Sensor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
            {
                return NotFound();
            }

            return alert;
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveAlert(long id)
        {
            await _alertService.ResolveAlertAsync(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Alert>> CreateAlert([FromBody] CreateAlertRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _alertService.CreateAlertAsync(
                request.SensorId,
                request.Message,
                request.Severity,
                request.ThresholdValue,
                request.ActualValue);

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(long id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetAlertStatistics()
        {
            var totalAlerts = await _context.Alerts.CountAsync();
            var resolvedAlerts = await _context.Alerts.CountAsync(a => a.IsResolved);
            var unresolvedAlerts = totalAlerts - resolvedAlerts;

            var severityCounts = await _context.Alerts
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentAlerts = await _context.Alerts
                .Where(a => a.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                .CountAsync();

            return Ok(new
            {
                Total = totalAlerts,
                Resolved = resolvedAlerts,
                Unresolved = unresolvedAlerts,
                Recent24Hours = recentAlerts,
                BySeverity = severityCounts
            });
        }
    }

    public class CreateAlertRequest
    {
        public int SensorId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info";
        public double? ThresholdValue { get; set; }
        public double? ActualValue { get; set; }
    }
}
