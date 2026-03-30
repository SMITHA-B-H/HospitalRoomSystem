using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Repositories;

namespace HospitalRoomAPI.Services
{
    public class DisplayService : IDisplayService
    {
        private readonly IDisplayRepository _repository;
        private readonly IHubContext<RoomHub> _hubContext;

        public DisplayService(
            IDisplayRepository repository,
            IHubContext<RoomHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        // ================= BUILD DISPLAY =================
        public async Task<object?> BuildRoomDisplay(string roomNumber)
        {
            var room = await _repository.GetRoomWithDetailsAsync(roomNumber);
            if (room == null) return null;

            var settings = await _repository.GetSettingsAsync(
                room.Floor.HospitalId,
                room.Id,
                room.FloorId);

            var beds = room.Beds
                .Where(b => b.Patient != null)
                .Select(b => new
                {
                    bedNumber = b.BedNumber,
                    occupied = true,
                    patient = new { name = b.Patient!.Name },
                    doctor = b.Patient!.Doctor != null ? new
                    {
                        name = b.Patient.Doctor.Name,
                        specialization = b.Patient.Doctor.Department,
                        photoUrl = b.Patient.Doctor.PhotoUrl
                    } : null
                })
                .ToList();

            var videos = await _repository.GetVideosAsync(
                room.Floor.HospitalId,
                room.Id,
                room.FloorId);

            var announcements = await _repository.GetAnnouncementsAsync(
                room.Floor.HospitalId,
                room.Id,
                room.FloorId);

            return new
            {
                roomNumber = room.RoomNumber,
                roomName = room.RoomName,
                logoUrl = settings?.LogoUrl,
                scrollingMessage = settings?.ScrollingMessage,
                scrollingSpeed = settings?.ScrollingSpeed ?? 5,
                adsVolume = settings?.AdsVolume ?? 50,
                showClock = settings?.ShowClock ?? false,
                beds,
                videos,
                announcements
            };
        }

        // ================= SINGLE ROOM PUSH =================
        public async Task PushRoomUpdate(string roomNumber)
        {
            var data = await BuildRoomDisplay(roomNumber);

            await _hubContext.Clients
                .Group(roomNumber)
                .SendAsync("RoomUpdated", data);
        }

        // ================= ✅ NEW METHOD (FIX) =================
        public async Task PushRoomUpdateByScope(int? roomId, int? floorId, int hospitalId)
        {
            var roomNumbers = await _repository.GetRoomNumbersByScope(roomId, floorId, hospitalId);

            foreach (var room in roomNumbers)
            {
                await PushRoomUpdate(room);
            }
        }
    }
}