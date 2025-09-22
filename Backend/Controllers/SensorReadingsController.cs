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
        public ActionResult<PaginatedResponse<SensorReading>> GetSensorReadings([FromQuery] PaginationRequest request)
        {
            var allReadings = _inMemoryService.GetAllRecentReadings(10000); // Get more data for pagination
            var totalCount = allReadings.Count();
            
            var paginatedReadings = allReadings
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new PaginatedResponse<SensorReading>(
                paginatedReadings, 
                request.Page, 
                request.PageSize, 
                totalCount);

            return Ok(response);
        }

        [HttpGet("by-sensor/{sensorId}")]
        public ActionResult<PaginatedResponse<SensorReading>> GetReadingsBySensor(int sensorId, [FromQuery] PaginationRequest request)
        {
            var allReadings = _inMemoryService.GetRecentReadings(sensorId, 10000); // Get more data for pagination
            var totalCount = allReadings.Count();
            
            var paginatedReadings = allReadings
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new PaginatedResponse<SensorReading>(
                paginatedReadings, 
                request.Page, 
                request.PageSize, 
                totalCount);

            return Ok(response);
        }

        [HttpGet("by-time-range")]
        public ActionResult<PaginatedResponse<SensorReading>> GetReadingsByTimeRange(
            DateTime startTime, 
            DateTime endTime,
            [FromQuery] PaginationRequest request)
        {
            var allReadings = _inMemoryService.GetReadingsByTimeRange(startTime, endTime);
            var totalCount = allReadings.Count();
            
            var paginatedReadings = allReadings
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var response = new PaginatedResponse<SensorReading>(
                paginatedReadings, 
                request.Page, 
                request.PageSize, 
                totalCount);

            return Ok(response);
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
