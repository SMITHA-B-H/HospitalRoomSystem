using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IDoctorService
    {
        Task<ApiResponse<List<Doctor>>> GetDoctorsAsync(int hospitalId);
        Task<ApiResponse<Doctor>> AddDoctorAsync(DoctorDto dto, int hospitalId, string role, string webRootPath);
        Task<ApiResponse<Doctor>> UpdateDoctorAsync(int id, DoctorDto dto, string webRootPath);
        Task<ApiResponse<object>> DeleteDoctorAsync(int id);
    }
}