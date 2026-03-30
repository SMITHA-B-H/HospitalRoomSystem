using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/display")]
    public class DisplayController : ControllerBase
    {
        private readonly IDisplayService _displayService;

        public DisplayController(DisplayService displayService)
        {
            _displayService = displayService;
        }

        // ? ONLY API HERE
        [HttpGet("room/{roomNumber}")]
        public async Task<IActionResult> GetRoomDisplay(string roomNumber)
        {
            var data = await _displayService.BuildRoomDisplay(roomNumber);

            if (data == null)
                return NotFound("Room not found");

            return Ok(data);
        }
    }
}