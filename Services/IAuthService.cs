using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> RegisterHospitalAsync(RegisterHospitalDto dto);
        Task<ApiResponse<object>> LoginAsync(LoginDto dto);
        Task<ApiResponse<object>> ForgotPasswordAsync(string email);
        Task<ApiResponse<object>> ResetPasswordAsync(string token, string newPassword);
    }
}