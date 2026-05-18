using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace HospitalRoomAPI.Services
{
    public interface IQueueService
    {
        Task<List<QueueEntry>> GetAllQueueAsync();
        Task<List<QueueEntry>> GetDoctorQueueAsync(int doctorId);
        Task<List<QueueEntry>> GetByStageAsync(string stage);

        Task<QueueEntry> AddToQueueAsync(QueueCreateDto dto);

        Task<QueueEntry?> CallNextAsync(int doctorId);
        Task<QueueEntry?> CompleteAsync(int id);
        Task<QueueEntry?> SkipAsync(int id);

        Task<QueueEntry?> MoveStageAsync(int id, string stage);
        Task<string> ResetQueueAsync();
        Task<QueueEntry?> RecallAsync(int id);

        Task<DisplayQueueDto?>GetDisplayQueueAsync(string displayNumber);

        Task<List<CentralDisplayDto>>GetCentralDisplayAsync();

        Task<ApiResponse<object>> ResetDoctorQueueAsync(int doctorId);

    }
}