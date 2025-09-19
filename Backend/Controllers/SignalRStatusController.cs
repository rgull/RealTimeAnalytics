using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeSensorTrack.Hubs;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRStatusController : ControllerBase
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly ISensorSimulationService _simulationService;

        public SignalRStatusController(
            IHubContext<SensorHub> hubContext,
            ISensorSimulationService simulationService)
        {
            _hubContext = hubContext;
            _simulationService = simulationService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                IsSignalRHubAvailable = _hubContext != null,
                IsSimulationRunning = _simulationService.IsRunning,
                HubUrl = "/sensorHub",
                Timestamp = DateTime.UtcNow,
                Message = "SignalR status check"
            });
        }

        [HttpPost("start-simulation")]
        public async Task<IActionResult> StartSimulation()
        {
            try
            {
                await _simulationService.StartSimulationAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Simulation started",
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }
        }

        [HttpPost("stop-simulation")]
        public async Task<IActionResult> StopSimulation()
        {
            try
            {
                await _simulationService.StopSimulationAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Simulation stopped",
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }
        }
    }
}
