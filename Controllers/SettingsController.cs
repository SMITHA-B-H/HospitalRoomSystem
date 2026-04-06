using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HospitalRoomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _service;

        public SettingsController(ISettingsService service)
        {
            _service = service;
        }

        // ================= DISPLAY =================
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetDisplaySettings(int roomId)
        {
            var result = await _service.GetDisplaySettings(roomId);
            return Ok(result);
        }

        // ================= SAVE =================
        [HttpPost("save")]
        public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsDto dto)
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var result = await _service.SaveSettings(dto, hospitalId);

            return Ok(result);
        }

        // ================= UPLOAD LOGO =================
        [Authorize]
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var result = await _service.UploadLogo(file, hospitalId);

            return Ok(result);
        }

        // ================= UPLOAD VIDEO =================
        [Authorize]
        [HttpPost("upload-video")]
        public async Task<IActionResult> UploadVideo([FromForm] UploadVideoDto dto)
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var result = await _service.UploadVideo(dto, hospitalId);

            return Ok(result);
        }

        // ================= DELETE VIDEO =================
        [Authorize]
        [HttpDelete("delete-video")]
        public async Task<IActionResult> DeleteVideo(string path)
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var result = await _service.DeleteVideo(path, hospitalId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            // ?? If you want hospital-based settings
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var result = await _service.GetSettingsByHospital(hospitalId);

            return Ok(result);
        }
    }
}