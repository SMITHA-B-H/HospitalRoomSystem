using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalRoomAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(IAuthRepository repo, IConfiguration config, IEmailService emailService)
        {
            _repo = repo;
            _config = config;
            _emailService = emailService;
        }

        public async Task<ApiResponse<object>> RegisterHospitalAsync(RegisterHospitalDto dto)
        {
            var existing = await _repo.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                return new ApiResponse<object> { Success = false, Message = "Email exists" };

            var hospital = new Hospital
            {
                Name = dto.HospitalName,
                Address = dto.Address,
                LogoUrl = "/logos/default.png"
            };

            hospital = await _repo.AddHospitalAsync(hospital);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "SuperAdmin",
                HospitalId = hospital.Id
            };

            await _repo.AddUserAsync(user);

            return new ApiResponse<object> { Success = true, Message = "Registered" };
        }

        public async Task<ApiResponse<object>> LoginAsync(LoginDto dto)
        {
            var user = await _repo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "Invalid" };

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return new ApiResponse<object> { Success = false, Message = "Invalid" };

            var token = GenerateToken(user);

            return new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    token,
                    role = user.Role,
                    hospitalId = user.HospitalId,
                    floorId = user.FloorId
                }
            };
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(string email)
        {
            var user = await _repo.GetUserByEmailAsync(email);
            if (user == null)
                return new ApiResponse<object> { Success = false, Message = "User not found" };

            var token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _repo.SaveChangesAsync();

            _emailService.SendEmail(email, "Reset", $"Token: {token}");

            return new ApiResponse<object> { Success = true };
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _repo.GetUserByEmailAsync(""); // simplified

            if (user == null)
                return new ApiResponse<object> { Success = false };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;

            await _repo.SaveChangesAsync();

            return new ApiResponse<object> { Success = true };
        }

        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("HospitalId", user.HospitalId.ToString())
            };

            var token = new JwtSecurityToken(
                jwt["Issuer"],
                jwt["Audience"],
                claims,
                expires: DateTime.Now.AddYears(100),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApiResponse<object>> RegisterFloorAdminAsync(
            RegisterFloorAdminDto dto,
            int hospitalId)
                {
                    var existing = await _repo.GetUserByEmailAsync(dto.Email);

                    if (existing != null)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Email already exists"
                        };
                    }

                    var user = new User
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                        Role = "FloorAdmin",
                        HospitalId = hospitalId,
                        FloorId = dto.FloorId
                    };

                    await _repo.AddUserAsync(user);

                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Floor Admin Created"
                    };
                 }

    }
}