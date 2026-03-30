using Microsoft.AspNetCore.SignalR;

namespace HospitalRoomAPI.Hubs
{
    public class RoomHub : Hub
    {
        public async Task JoinRoom(string roomNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomNumber);

            await Clients.Caller.SendAsync(
                "JoinedRoom",
                $"Connected to Room {roomNumber}"
            );
        }

        public async Task LeaveRoom(string roomNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomNumber);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            Console.WriteLine($"Disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(ex);
        }
    }
}