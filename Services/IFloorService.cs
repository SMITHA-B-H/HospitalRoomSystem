using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IFloorService
    {
        Task<ApiResponse<List<Floor>>> GetFloorsAsync(int hospitalId);
        Task<ApiResponse<Floor>> AddFloorAsync(CreateFloorDto dto, int hospitalId);
        Task<ApiResponse<object>> UpdateFloorAsync(int id, UpdateFloorDto dto);
        Task<ApiResponse<object>> DeleteFloorAsync(int id);
    }
}