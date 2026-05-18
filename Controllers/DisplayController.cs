using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/display")]
    public class DisplayController : ControllerBase
    {
        private readonly IDisplayService _displayService;

        public DisplayController(IDisplayService displayService)
        {
            _displayService = displayService;
        }

        // ================= ROOM DATA =================
        [HttpGet("room/{roomNumber}")]
        public async Task<IActionResult> GetRoomDisplay(string roomNumber)
        {
            var data = await _displayService.BuildRoomDisplay(roomNumber);

            if (data == null)
                return NotFound("Room not found");

            FixUrls(data);

            return Ok(data);
        }

        // ================= URL FIX =================
        private void FixUrls(RoomDisplay dto)
        {
            dto.logoUrl = Convert(dto.logoUrl);

            // Beds
            foreach (var bed in dto.beds ?? new List<BedDisplayDto>())
            {
                bed.doctorPhoto = Convert(bed.doctorPhoto);
            }

            // Videos
            dto.videos = dto.videos?
                .Select(v => Convert(v))
                .ToList();

            // ? FIXED: DTO instead of PatientAnnouncement
            foreach (var a in dto.announcements ?? new List<AnnouncementDisplayDto>())
            {
                a.PosterUrl = Convert(a.PosterUrl);
                a.VideoUrl = Convert(a.VideoUrl);
            }
        }

        private string Convert(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            // already full URL
            if (url.StartsWith("http"))
                return url;

            return $"http://192.168.1.12:5000{url}";
        }
    }
}