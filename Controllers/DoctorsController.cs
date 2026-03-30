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
        private readonly IWebHostEnvironment _env;

        public DoctorsController(IDoctorService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        private int GetHospitalId()
        {
            return int.Parse(User.FindFirst("HospitalId")!.Value);
        }

        private string GetRole()
        {
            return User.FindFirst(ClaimTypes.Role)!.Value;
        }

        // ================= GET =================

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var result = await _service.GetDoctorsAsync(GetHospitalId());
            return Ok(result);
        }

        // ================= ADD =================

        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto dto)
        {
            var result = await _service.AddDoctorAsync(
                dto,
                GetHospitalId(),
                GetRole(),
                _env.WebRootPath!
            );

            if (!result.Success)
                return Forbid();

            return Ok(result);
        }

        // ================= UPDATE =================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromForm] DoctorDto dto)
        {
            var result = await _service.UpdateDoctorAsync(
                id,
                dto,
                _env.WebRootPath!
            );

            if (!result.Success)
                return NotFound();

            return Ok(result);
        }

        // ================= DELETE =================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var result = await _service.DeleteDoctorAsync(id);

            if (!result.Success)
                return NotFound();

            return Ok(result);
        }
    }
}