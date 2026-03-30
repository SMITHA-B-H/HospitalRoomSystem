using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
	public interface IDisplayRepository
	{
		Task<Room?> GetRoomWithDetailsAsync(string roomNumber);
		Task<List<string>> GetVideosAsync(int hospitalId, int roomId, int floorId);
		Task<List<object>> GetAnnouncementsAsync(int hospitalId, int roomId, int floorId);
		Task<Setting?> GetSettingsAsync(int hospitalId, int roomId, int floorId);
		Task<List<string>> GetRoomNumbersByScope(int? roomId, int? floorId, int hospitalId);
	}
}