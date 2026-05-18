using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IDoctorRepository _repo;

        public AuthController(
            IAuthService service,
            IDoctorRepository repo
        )
        {
            _service = service;
            _repo = repo;
        }

        // =========================================
        // REGISTER HOSPITAL
        // =========================================
        [HttpPost("register-hospital")]
        public async Task<IActionResult> Register(
            RegisterHospitalDto dto
        )
        {
            var res =
                await _service.RegisterHospitalAsync(dto);

            if (!res.Success)
                return BadRequest(res.Message);

            return Ok(res);
        }

        // =========================================
        // LOGIN
        // =========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDto dto
        )
        {
            var res =
                await _service.LoginAsync(dto);

            if (!res.Success)
                return Unauthorized(res.Message);

            return Ok(res);
        }

        // =========================================
        // FORGOT PASSWORD
        // =========================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot(
            ForgotPasswordDto dto
        )
        {
            var res =
                await _service.ForgotPasswordAsync(
                    dto.Email
                );

            return Ok(res);
        }

        // =========================================
        // RESET PASSWORD
        // =========================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset(
            ResetPasswordDto dto
        )
        {
            var res =
                await _service.ResetPasswordAsync(
                    dto.Token,
                    dto.NewPassword
                );

            return Ok(res);
        }

        // =========================================
        // EMPLOYEE LOGIN BY EMPLOYEE ID
        // api/auth/employee/EMP001
        // =========================================
        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetByEmployee(
            string id
        )
        {
            var emp =
                await _repo.GetByEmployeeIdAsync(id);

            if (emp == null)
                return NotFound(
                    "Employee Not Found"
                );

            return Ok(new
            {
                emp.Id,
                emp.EmployeeId,
                emp.Name,
                emp.Role,
                emp.Department,
                emp.PhotoUrl
            });
        }


        //=========================================
        // REGISTER FLOOR ADMIN
        //=========================================
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("register-floor-admin")]
        public async Task<IActionResult> RegisterFloorAdmin(RegisterFloorAdminDto dto)
        {
            var hospitalIdClaim = User.FindFirst("HospitalId");

            if (hospitalIdClaim == null)
            {
                return Unauthorized();
            }

            int hospitalId = int.Parse(hospitalIdClaim.Value);

            var result = await _service.RegisterFloorAdminAsync(dto, hospitalId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}