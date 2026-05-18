using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Repositories
{
    public interface IQueueRepository
    {
        Task<List<QueueEntry>> GetAllQueueAsync();
        Task<List<QueueEntry>> GetDoctorQueueAsync(int doctorId);
        Task<List<QueueEntry>> GetByStageAsync(string stage);

        Task<QueueEntry?> GetNextWaitingAsync(int doctorId);
        Task<QueueEntry?> GetByIdAsync(int id);

        Task<int> GetLastTokenByDoctorAsync(int doctorId, DateTime date);

        Task AddAsync(QueueEntry entry);
        Task SaveAsync();

        Task ResetQueueAsync();

        Task<List<QueueEntry>>GetDisplayQueueAsync(string displayNumber);

        Task<List<CentralDisplayDto>>GetCentralDisplayAsync();

        Task<ApiResponse<object>> ResetDoctorQueueAsync(int doctorId);


    }
}