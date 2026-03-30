using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Hubs;

namespace HospitalRoomAPI.Services
{
    public class SignalRService
    {
        private readonly IHubContext<RoomHub> _hub;

        public SignalRService(IHubContext<RoomHub> hub)
        {
            _hub = hub;
        }

        public async Task SendRoomUpdate(string roomNumber, object data)
        {
            await _hub.Clients.Group(roomNumber)
                .SendAsync("RoomUpdated", data);
        }

        public async Task Broadcast(string eventName, object data)
        {
            await _hub.Clients.All.SendAsync(eventName, data);
        }
    }
}