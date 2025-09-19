using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Models;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorReadingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInMemoryDataService _inMemoryService;
        private readonly ILogger<SensorReadingsController> _logger;

        public SensorReadingsController(
            ApplicationDbContext context,
            IInMemoryDataService inMemoryService,
            ILogger<SensorReadingsController> logger)
        {
            _context = context;
            _inMemoryService = inMemoryService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SensorReading>> GetSensorReadings(int count = 1000)
        {
            var readings = _inMemoryService.GetAllRecentReadings(count);
            return Ok(readings);
        }

        [HttpGet("by-sensor/{sensorId}")]
        public ActionResult<IEnumerable<SensorReading>> GetReadingsBySensor(int sensorId, int count = 100)
        {
            var readings = _inMemoryService.GetRecentReadings(sensorId, count);
            return Ok(readings);
        }

        [HttpGet("by-time-range")]
        public ActionResult<IEnumerable<SensorReading>> GetReadingsByTimeRange(
            DateTime startTime, 
            DateTime endTime)
        {
            var readings = _inMemoryService.GetReadingsByTimeRange(startTime, endTime);
            return Ok(readings);
        }

        [HttpGet("count")]
        public ActionResult<object> GetReadingCount()
        {
            var count = _inMemoryService.GetReadingCount();
            return Ok(new { Count = count });
        }

        [HttpPost]
        public async Task<ActionResult<SensorReading>> PostSensorReading(SensorReading reading)
        {
            reading.Timestamp = DateTime.UtcNow;
            _context.SensorReadings.Add(reading);
            await _context.SaveChangesAsync();

            // Add to in-memory storage
            _inMemoryService.AddReading(reading);

            return CreatedAtAction("GetSensorReading", new { id = reading.Id }, reading);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SensorReading>> GetSensorReading(long id)
        {
            var reading = await _context.SensorReadings
                .Include(sr => sr.Sensor)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (reading == null)
            {
                return NotFound();
            }

            return reading;
        }
    }
}
