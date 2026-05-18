using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _service;

        public DoctorsController(IDoctorService service)
        {
            _service = service;
        }

        // ================= GET =================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetDoctors(int hospitalId)
        {
            var result =
               await _service.GetDoctorsAsync(hospitalId);

            return Ok(result);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto dto)
        {
            var hospitalIdClaim = User.FindFirst("HospitalId")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (hospitalIdClaim == null || role == null)
                return Unauthorized();

            int hospitalId = int.Parse(hospitalIdClaim);

            var result = await _service.AddDoctorAsync(dto, hospitalId, role);

            if (!result.Success)
                return StatusCode(403, result);

            return Ok(result);
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromForm] DoctorDto dto)
        {
            if (id <= 0)
                return BadRequest("Invalid doctor ID");

            var result = await _service.UpdateDoctorAsync(id, dto);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid doctor ID");

            var result = await _service.DeleteDoctorAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("update-display/{id}")]
        public async Task<IActionResult>
            UpdateDisplay(
                int id,
                UpdateDisplayDto dto)
                    {
                        var updated =
                            await _service
                            .UpdateDisplayAsync(
                                id,
                                dto.DisplayNumber);

                        if (!updated)
                            return NotFound(
                                "Doctor not found");

                        return Ok(
                            "Display updated successfully");
                    }


    }
}