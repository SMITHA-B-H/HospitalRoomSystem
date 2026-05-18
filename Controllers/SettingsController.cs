using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Hubs;

namespace HospitalRoomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _service;

        private readonly IHubContext<RoomHub> _hub;

        public SettingsController(
            ISettingsService service,
            IHubContext<RoomHub> hub)
        {
            _service = service;
            _hub = hub;
        }

        // ================= DISPLAY =================

        [HttpGet("{roomId}")]
        public async Task<IActionResult>
        GetDisplaySettings(int roomId)
        {
            var result =
                await _service
                    .GetDisplaySettings(roomId);

            return Ok(result);
        }

        // ================= SAVE SETTINGS =================

        [HttpPost("save")]
        public async Task<IActionResult>
        SaveSettings(
            [FromBody] SaveSettingsDto dto)
        {
            int hospitalId =
                int.Parse(
                    User.FindFirst("HospitalId")!
                        .Value);

            var result =
                await _service.SaveSettings(
                    dto,
                    hospitalId);

            // ================= SIGNALR =================

            var publicSettings =
                await _service
                    .GetPublicSettingsAsync();

            await _hub.Clients.All.SendAsync(
                "PublicSettingsUpdated",
                publicSettings);

            return Ok(result);
        }

        // ================= UPLOAD LOGO =================

        [HttpPost("upload-logo")]
        public async Task<IActionResult>
        UploadLogo(IFormFile file)
        {
            int hospitalId =
                int.Parse(
                    User.FindFirst("HospitalId")!
                        .Value);

            var result =
                await _service.UploadLogo(
                    file,
                    hospitalId);

            // ================= SIGNALR =================

            var publicSettings =
                await _service
                    .GetPublicSettingsAsync();

            await _hub.Clients.All.SendAsync(
                "PublicSettingsUpdated",
                publicSettings);

            return Ok(result);
        }

        // ================= UPLOAD VIDEO =================

        [HttpPost("upload-video")]
        public async Task<IActionResult>
        UploadVideo(
            [FromForm] UploadVideoDto dto)
        {
            int hospitalId =
                int.Parse(
                    User.FindFirst("HospitalId")!
                        .Value);

            var result =
                await _service.UploadVideo(
                    dto,
                    hospitalId);

            // ================= SIGNALR =================

            var publicSettings =
                await _service
                    .GetPublicSettingsAsync();

            await _hub.Clients.All.SendAsync(
                "PublicSettingsUpdated",
                publicSettings);

            return Ok(result);
        }

        // ================= DELETE VIDEO =================

        [HttpDelete("delete-video")]
        public async Task<IActionResult>
        DeleteVideo(string path)
        {
            int hospitalId =
                int.Parse(
                    User.FindFirst("HospitalId")!
                        .Value);

            var result =
                await _service.DeleteVideo(
                    path,
                    hospitalId);

            // ================= SIGNALR =================

            var publicSettings =
                await _service
                    .GetPublicSettingsAsync();

            await _hub.Clients.All.SendAsync(
                "PublicSettingsUpdated",
                publicSettings);

            return Ok(result);
        }

        // ================= GET SETTINGS =================

        [HttpGet]
        public async Task<IActionResult>
        GetSettings()
        {
            int hospitalId =
                int.Parse(
                    User.FindFirst("HospitalId")!
                        .Value);

            var result =
                await _service
                    .GetSettingsByHospital(
                        hospitalId);

            return Ok(result);
        }

        // ================= PUBLIC SETTINGS =================

        [HttpGet("PublicSettings")]
        [AllowAnonymous]
        public async Task<IActionResult>
        GetPublicSettings()
        {
            var result =
                await _service
                    .GetPublicSettingsAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}