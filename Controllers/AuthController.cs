using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register-hospital")]
        public async Task<IActionResult> Register(RegisterHospitalDto dto)
        {
            var res = await _service.RegisterHospitalAsync(dto);
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var res = await _service.LoginAsync(dto);
            if (!res.Success) return Unauthorized(res.Message);
            return Ok(res);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot(ForgotPasswordDto dto)
        {
            var res = await _service.ForgotPasswordAsync(dto.Email);
            return Ok(res);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset(ResetPasswordDto dto)
        {
            var res = await _service.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(res);
        }
    }
}