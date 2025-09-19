using Microsoft.AspNetCore.SignalR;
using RealTimeSensorTrack.Models;
using RealTimeSensorTrack.Services;

namespace RealTimeSensorTrack.Hubs
{
    public class SensorHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public SensorHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SubscribeToSensor(int sensorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"sensor_{sensorId}");
        }

        public async Task UnsubscribeFromSensor(int sensorId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sensor_{sensorId}");
        }

        public async Task StartSimulation()
        {
            using var scope = _serviceProvider.CreateScope();
            var simulationService = scope.ServiceProvider.GetRequiredService<ISensorSimulationService>();
            await simulationService.StartSimulationAsync();
            await Clients.Caller.SendAsync("SimulationStarted", "Auto sensor reading generator started");
        }

        public async Task StopSimulation()
        {
            using var scope = _serviceProvider.CreateScope();
            var simulationService = scope.ServiceProvider.GetRequiredService<ISensorSimulationService>();
            await simulationService.StopSimulationAsync();
            await Clients.Caller.SendAsync("SimulationStopped", "Auto sensor reading generator stopped");
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
