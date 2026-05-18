using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;

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

        // ✅ FINAL WORKING METHOD
        public async Task<RoomDisplay?> BuildRoomDisplay(string roomNumber)
        {
            var room = await _repository.GetRoomWithDetailsAsync(roomNumber);
            if (room == null) return null;

            var settings = await _repository.GetSettingsAsync(
                room.Floor.HospitalId,
                room.Id,
                room.FloorId);

            // ✅ USE YOUR EXISTING MODELS (PascalCase)
            var beds = room.Beds
                .Where(b => b.Patient != null)
                .Select(b => new BedDisplayDto
                {
                    bedNumber = b.BedNumber,
                    status = b.Status,

                    patientName = b.Patient.Name,

                    doctorName = b.Patient.Doctor != null
                        ? b.Patient.Doctor.Name
                        : "",

                    doctorPhoto = b.Patient.Doctor != null
                        ? b.Patient.Doctor.PhotoUrl
                        : "",
                    doctorDepartment = b.Patient.Doctor != null
                        ? b.Patient.Doctor.Department
                        : ""
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

            return new RoomDisplay
            {
                roomNumber = room.RoomNumber,
                roomName = room.RoomName,

                logoUrl = settings?.LogoUrl ?? "",
                scrollingMessage = settings?.ScrollingMessage ?? "",
                scrollingSpeed = settings?.ScrollingSpeed ?? 5,
                adsVolume = settings?.AdsVolume ?? 50,
                showClock = settings?.ShowClock ?? false,

                beds = beds, // ✅ directly using DB model
                videos = videos.ToList(),
                announcements = announcements.ToList()
            };
        }

        public async Task PushRoomUpdate(string roomNumber)
        {
            var data = await BuildRoomDisplay(roomNumber);

            await _hubContext.Clients
                .Group(roomNumber)
                .SendAsync("RoomUpdated", data);
        }

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