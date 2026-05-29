using Microsoft.AspNetCore.SignalR;

using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Services
{
    public class DisplayService : IDisplayService
    {
        private readonly IDisplayRepository _repository;

        private readonly IHubContext<RoomHub>
            _hubContext;

        public DisplayService(

            IDisplayRepository repository,

            IHubContext<RoomHub> hubContext)
        {
            _repository = repository;

            _hubContext = hubContext;
        }

        // =====================================================
        // BUILD SINGLE ROOM DISPLAY
        // =====================================================

        public async Task<RoomDisplay?>
            BuildRoomDisplay(
                string roomNumber)
        {
            // LOAD ONLY REQUIRED ROOM

            var room =
                await _repository
                    .GetRoomWithDetailsAsync(
                        roomNumber);

            if (room == null)
                return null;

            // SETTINGS

            var settings =
                await _repository
                    .GetSettingsAsync(

                        room.Floor.HospitalId,

                        room.Id,

                        room.FloorId);

            // BEDS

            var beds = room.Beds

                .Where(b =>

                    b.Patient != null ||

                    (b.Status != null &&
                     b.Status.ToLower()
                        == "booked"))

                .Select(b =>
                    new BedDisplayDto
                    {
                        bedNumber =
                            b.BedNumber,

                        status =
                            b.Status ?? "",

                        patientName =
                            b.Patient != null
                                ? b.Patient.Name
                                : "Booked",

                        PatientType =
                            b.Patient != null
                                ? b.Patient.PatientType
                                : "N/A",

                        doctorName =
                            b.Patient?.Doctor?.Name
                            ?? "",

                        doctorPhoto =
                            b.Patient?.Doctor?.PhotoUrl
                            ?? "",

                        doctorDepartment =
                            b.Patient?.Doctor?.Department
                            ?? ""
                    })

                .ToList();

            // VIDEOS

            var videos =
                await _repository
                    .GetVideosAsync(

                        room.Floor.HospitalId,

                        room.Id,

                        room.FloorId);

            // ANNOUNCEMENTS

            var announcements =
                await _repository
                    .GetAnnouncementsAsync(

                        room.Floor.HospitalId,

                        room.Id,

                        room.FloorId);

            // RETURN DTO

            return new RoomDisplay
            {
                roomNumber =
                    room.RoomNumber,

                roomName =
                    room.RoomName,

                logoUrl =
                    settings?.LogoUrl ?? "",

                scrollingMessage =
                    settings?.ScrollingMessage
                    ?? "",

                scrollingSpeed =
                    settings?.ScrollingSpeed
                    ?? 5,

                adsVolume =
                    settings?.AdsVolume
                    ?? 50,

                showClock =
                    settings?.ShowClock
                    ?? false,

                beds = beds,

                videos =
                    videos.ToList(),

                announcements =
                    announcements.ToList()
            };
        }

        // =====================================================
        // PUSH SINGLE ROOM UPDATE
        // =====================================================

        public async Task PushRoomUpdate(
            string roomNumber)
        {
            try
            {
                Console.WriteLine(

                    $"🔥 PushRoomUpdate: {roomNumber}"
                );

                var data =
                    await BuildRoomDisplay(
                        roomNumber);

                if (data == null)
                {
                    Console.WriteLine(

                        $"❌ No room data: {roomNumber}"
                    );

                    return;
                }

                // SEND ONLY TO ROOM GROUP

                await _hubContext
                    .Clients
                    .Group(roomNumber)
                    .SendAsync(
                        "RoomUpdated",
                        data);

                Console.WriteLine(

                    $"✅ SignalR sent to {roomNumber}"
                );
            }

            catch (Exception ex)
            {
                Console.WriteLine(

                    $"❌ PushRoomUpdate Error: {ex.Message}"
                );
            }
        }

        // =====================================================
        // PUSH BY SCOPE
        // =====================================================

        public async Task PushRoomUpdateByScope(

            int? roomId,

            int? floorId,

            int hospitalId)
        {
            try
            {
                Console.WriteLine(
                    "🔥 PushRoomUpdateByScope"
                );

                // GET ONLY REQUIRED ROOMS

                var roomNumbers =
                    await _repository
                        .GetRoomNumbersByScope(

                            roomId,

                            floorId,

                            hospitalId);

                // SAFETY

                if (roomNumbers == null ||
                    !roomNumbers.Any())
                {
                    Console.WriteLine(
                        "❌ No rooms found"
                    );

                    return;
                }

                // UPDATE ONLY AFFECTED ROOMS

                foreach (var roomNumber
                    in roomNumbers.Distinct())
                {
                    await PushRoomUpdate(
                        roomNumber);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(

                    $"❌ PushRoomUpdateByScope Error: {ex.Message}"
                );
            }
        }
    }
}