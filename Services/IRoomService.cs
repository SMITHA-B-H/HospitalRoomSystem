using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Services
{
    public interface IRoomService
    {
        Task<ApiResponse<object>> GetRoomsAsync(string role, int hospitalId, int? floorId);

        Task<ApiResponse<Room>> CreateRoomAsync(
            Room room,
            string role,
            int hospitalId,
            int? floorId
        );

        Task<ApiResponse<Patient>> AssignPatientAsync(AssignPatientDto dto);

        Task<ApiResponse<object>> DischargePatientAsync(int bedId);

        Task<List<object>> GetRoomsByFloorAsync(int floorId);

        Task<ApiResponse<object>> DeleteRoomAsync(
            int id,
            string role,
            int hospitalId,
            int? floorId
        );

        Task<ApiResponse<Room>> UpdateRoomAsync(
            int id,
            Room updatedRoom,
            string role,
            int hospitalId,
            int? floorId
        );

        Task<ApiResponse<object>> BookBedAsync(int bedId);

        Task<ApiResponse<object>> CancelBookingAsync(int bedId);

        Task<ApiResponse<object>> UpdatePatientAsync(int patientId,UpdatePatientDto dto);


    }
}