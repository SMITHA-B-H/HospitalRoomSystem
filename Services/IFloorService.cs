using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IFloorService
    {
        Task<ApiResponse<List<Floor>>> GetFloorsAsync(int hospitalId);
        Task<ApiResponse<Floor>> AddFloorAsync(CreateFloorDto dto, int hospitalId);
        Task<ApiResponse<Floor>> UpdateFloorAsync(int id, UpdateFloorDto dto);
        Task<ApiResponse<Floor>> DeleteFloorAsync(int id);
    }
}