using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Services
{
    public interface ISettingsService
    {
        Task<ApiResponse<object>> SaveSettings(SaveSettingsDto dto, int hospitalId);

        Task<ApiResponse<object>> GetDisplaySettings(int roomId);

        Task<ApiResponse<string>> UploadLogo(IFormFile file, int hospitalId);

        Task<ApiResponse<string>> UploadVideo(UploadVideoDto dto, int hospitalId);

        Task<ApiResponse<object>> DeleteVideo(string path, int hospitalId);
    }
}