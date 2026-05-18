using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using System.Linq;

namespace HospitalRoomAPI.Hubs
{
    public class RoomHub : Hub
    {
        private readonly AppDbContext _context;

        public RoomHub(AppDbContext context)
        {
            _context = context;
        }

        // ?? JOIN ROOM (STRICT SINGLE ENTRY)
        public async Task JoinRoom(string roomNumber, string deviceId, string deviceName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomNumber);

            // ?? CHECK BY ROOM (NOT DEVICE)
            var device = _context.DisplayDevices
                .FirstOrDefault(x => x.RoomNumber == roomNumber);

            if (device == null)
            {
                // ? FIRST TIME ? CREATE
                device = new DisplayDevice
                {
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    RoomNumber = roomNumber,
                    ConnectionId = Context.ConnectionId,
                    IsOnline = true,
                    LastSeen = DateTime.Now
                };

                _context.DisplayDevices.Add(device);
            }
            else
            {
                // ?? ROOM EXISTS ? ONLY UPDATE (NO NEW ROW)
                device.DeviceId = deviceId; // optional (if device replaced)
                device.DeviceName = deviceName;
                device.ConnectionId = Context.ConnectionId;
                device.IsOnline = true;
                device.LastSeen = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"? {deviceName} connected to Room {roomNumber}");

            // ?? LIVE UPDATE
            await Clients.All.SendAsync("DeviceStatusChanged", new
            {
                device.DeviceName,
                device.RoomNumber,
                device.IsOnline,
                device.LastSeen
            });

            await Clients.Caller.SendAsync("JoinedRoom", $"Connected to Room {roomNumber}");
        }

        // ?? LEAVE ROOM
        public async Task LeaveRoom(string roomNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomNumber);
        }

        // ?? CONNECT LOG
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }


        public async Task JoinDoctorGroup(int doctorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
        }

        // ?? DISCONNECT ? MARK OFFLINE
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var device = _context.DisplayDevices
                .FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (device != null)
            {
                device.IsOnline = false;
                device.LastSeen = DateTime.Now;

                await _context.SaveChangesAsync();

                Console.WriteLine($"? {device.DeviceName} disconnected");

                // ?? REAL-TIME UPDATE
                await Clients.All.SendAsync("DeviceStatusChanged", new
                {
                    device.DeviceName,
                    device.RoomNumber,
                    device.IsOnline,
                    device.LastSeen
                });
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}