using Microsoft.AspNetCore.SignalR;
using RealTimeSensorTrack.Models;

namespace RealTimeSensorTrack.Hubs
{
    public class SensorHub : Hub
    {
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
