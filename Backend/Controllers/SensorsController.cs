using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeSensorTrack.Data;
using RealTimeSensorTrack.Models;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInMemoryDataService _inMemoryService;
        private readonly ILogger<SensorsController> _logger;

        public SensorsController(
            ApplicationDbContext context,
            IInMemoryDataService inMemoryService,
            ILogger<SensorsController> logger)
        {
            _context = context;
            _inMemoryService = inMemoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<Sensor>>> GetSensors([FromQuery] PaginationRequest request)
        {
            var query = _context.Sensors
                .Include(x => x.SensorReadings)
                .Include(x => x.SensorStatistics)
                .Where(s => s.IsActive)
                .AsQueryable();

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var sensors = await query
                .OrderBy(s => s.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var response = new PaginatedResponse<Sensor>(
                sensors, 
                request.Page, 
                request.PageSize, 
                totalCount);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sensor>> GetSensor(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);

            if (sensor == null)
            {
                return NotFound();
            }

            return sensor;
        }

        [HttpPost]
        public async Task<ActionResult<Sensor>> PostSensor(Sensor sensor)
        {
            sensor.CreatedAt = DateTime.UtcNow;
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSensor", new { id = sensor.Id }, sensor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSensor(int id, Sensor sensor)
        {
            if (id != sensor.Id)
            {
                return BadRequest();
            }

            _context.Entry(sensor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SensorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
            {
                return NotFound();
            }

            // sensor.IsActive = false;
             _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/readings")]
        public ActionResult<PaginatedResponse<SensorReading>> GetSensorReadings(int id, [FromQuery] PaginationRequest request)
        {
            var allReadings = _inMemoryService.GetRecentReadings(id, 10000); // Get more data for pagination
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

        [HttpGet("{id}/statistics")]
        public ActionResult<object> GetSensorStatistics(int id)
        {
          //  var recentReadings = _inMemoryService.GetRecentReadings(id, 1000).ToList();
            var recentReadings = _context.SensorReadings.Where(x => x.SensorId == id).ToList();    

            if (!recentReadings.Any())
            {
                return NotFound("No recent readings found for this sensor");
            }

            var values = recentReadings.Select(r => r.Value).ToList();
            var statistics = new
            {
                SensorId = id,
                Count = values.Count,
                Min = values.Min(),
                Max = values.Max(),
                Average = values.Average(),
                StandardDeviation = CalculateStandardDeviation(values),
                LastReading = recentReadings.OrderByDescending(r => r.Timestamp).First(),
                TimeRange = new
                {
                    Start = recentReadings.Min(r => r.Timestamp),
                    End = recentReadings.Max(r => r.Timestamp)
                }
            };

            return Ok(statistics);
        }

        private bool SensorExists(int id)
        {
            return _context.Sensors.Any(e => e.Id == id);
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            var mean = values.Average();
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            return Math.Sqrt(variance);
        }
    }
}
