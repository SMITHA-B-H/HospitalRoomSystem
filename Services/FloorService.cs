using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public class FloorService : IFloorService
    {
        private readonly IFloorRepository _repository;
        private readonly IDisplayService _displayService;

        public FloorService(IFloorRepository repository, IDisplayService displayService)
        {
            _repository = repository;
            _displayService = displayService;
        }

        public async Task<ApiResponse<List<Floor>>> GetFloorsAsync(int hospitalId)
        {
            var floors = await _repository.GetFloorsAsync(hospitalId);

            return new ApiResponse<List<Floor>>
            {
                Success = true,
                Data = floors
            };
        }

        public async Task<ApiResponse<Floor>> AddFloorAsync(CreateFloorDto dto, int hospitalId)
        {

            var existingFloor = (await _repository.GetFloorsAsync(hospitalId))
            .FirstOrDefault(f =>
                f.FloorName!.ToLower() == dto.FloorName.ToLower());

            if (existingFloor != null)
            {
                return new ApiResponse<Floor>
                {
                    Success = false,
                    Message = "Floor already exists."
                };
            }

            var floor = new Floor
            {
                FloorName = dto.FloorName,
                HospitalId = hospitalId
            };

            await _repository.AddFloorAsync(floor);
            await _repository.SaveChangesAsync();

            var rooms = await _repository.GetRoomNumbersByFloorIdAsync(floor.Id);
            foreach (var room in rooms)
            {
                if (!string.IsNullOrEmpty(room))
                    await _displayService.PushRoomUpdate(room);
            }

            return new ApiResponse<Floor>
            {
                Success = true,
                Data = floor
            };
        }

        public async Task<ApiResponse<Floor>> UpdateFloorAsync(int id, UpdateFloorDto dto)
        {
            var floor = await _repository.GetFloorByIdAsync(id);

            if (floor == null)
                return new ApiResponse<Floor> { Success = false, Message = "Not found" };

            floor.FloorName = dto.FloorName;

            var admin = floor.Users?.FirstOrDefault(u => u.Role == "FloorAdmin");

            if (admin != null)
            {
                admin.Name = dto.AdminName;
                admin.Email = dto.AdminEmail;

                if (!string.IsNullOrEmpty(dto.Password))
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _repository.SaveChangesAsync();

            var rooms = await _repository.GetRoomNumbersByFloorIdAsync(id);
            foreach (var room in rooms)
            {
                if (!string.IsNullOrEmpty(room))
                    await _displayService.PushRoomUpdate(room);
            }

            return new ApiResponse<Floor>
            {
                Success = true,
                Data = floor
            };
        }

        public async Task<ApiResponse<Floor>> DeleteFloorAsync(int id)
        {
            var floor = await _repository.GetFloorByIdAsync(id);

            if (floor == null)
                return new ApiResponse<Floor> { Success = false, Message = "Not found" };

            // Prevent delete if rooms exist
            if (floor.Rooms != null && floor.Rooms.Any())
            {
                return new ApiResponse<Floor>
                {
                    Success = false,
                    Message = "Floor cannot be deleted because rooms exist."
                };
            }

            var rooms = await _repository.GetRoomNumbersByFloorIdAsync(id);

            await _repository.DeleteFloorAsync(floor);
            await _repository.SaveChangesAsync();

            foreach (var room in rooms)
            {
                if (!string.IsNullOrEmpty(room))
                    await _displayService.PushRoomUpdate(room);
            }

            return new ApiResponse<Floor>
            {
                Success = true,
                Message = "Floor deleted successfully",
                Data = floor
            };
        }
    }
}