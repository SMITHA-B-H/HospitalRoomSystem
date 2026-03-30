using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Repositories
{
	public interface ISettingsRepository
	{
		Task<Setting?> GetSettings(int hospitalId, int? floorId, int? roomId);
		Task<Setting> SaveSettings(Setting settings);

		Task<List<string>> GetRoomNumbers(int? roomId, int? floorId, int hospitalId);

		Task<Room?> GetRoomWithDetails(int roomId);
		Task<List<string>> GetVideos(int hospitalId, int? floorId, int? roomId);
		Task<List<PatientAnnouncement>> GetAnnouncements(int hospitalId, int? floorId, int? roomId);

		Task<string> UploadLogo(IFormFile file, int hospitalId);
		Task<string> UploadVideo(UploadVideoDto dto, int hospitalId);
		Task DeleteVideo(string path);
	}
}