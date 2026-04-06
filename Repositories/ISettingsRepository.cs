using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Repositories
{
	public interface ISettingsRepository
	{
		Task<Setting?> GetSettings(int hospitalId, int? roomId, int? floorId);
		Task<Setting> SaveSettings(Setting settings);

		Task<List<string>> GetRoomNumbers(int? roomId, int? floorId, int hospitalId);

		Task<Room?> GetRoomWithDetails(int roomId);
		Task<List<string>> GetVideos(int hospitalId, int? floorId, int? roomId);
		Task<List<PatientAnnouncement>> GetAnnouncements(int hospitalId, int? floorId, int? roomId);

        Task SaveLogo(string url, int hospitalId);
        Task<string> SaveVideo(string url, int hospitalId, UploadVideoDto dto);
        Task DeleteVideo(string path);
	}
}