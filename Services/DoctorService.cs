using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.Repositories;

namespace HospitalRoomAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _repository;
        private readonly IDisplayService _displayService;

        public DoctorService(IDoctorRepository repository, IDisplayService displayService)
        {
            _repository = repository;
            _displayService = displayService;
        }

        public async Task<ApiResponse<List<Doctor>>> GetDoctorsAsync(int hospitalId)
        {
            var doctors = await _repository.GetDoctorsAsync(hospitalId);

            return new ApiResponse<List<Doctor>>
            {
                Success = true,
                Data = doctors
            };
        }

        public async Task<ApiResponse<Doctor>> AddDoctorAsync(
            DoctorDto dto,
            int hospitalId,
            string role,
            string webRootPath)
        {
            if (role != "SuperAdmin")
                return new ApiResponse<Doctor> { Success = false, Message = "Forbidden" };

            string? photoPath = null;

            if (dto.Photo != null)
            {
                var uploads = Path.Combine(webRootPath, "uploads");

                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Photo.CopyToAsync(stream);

                photoPath = "/uploads/" + fileName;
            }

            var doctor = new Doctor
            {
                Name = dto.Name,
                Department = dto.Department,
                PhotoUrl = photoPath,
                HospitalId = hospitalId
            };

            await _repository.AddAsync(doctor);
            await _repository.SaveChangesAsync();

            var rooms = await _repository.GetDoctorRoomNumbersAsync(doctor.Id);

            foreach (var room in rooms)
                await _displayService.PushRoomUpdate(room);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        public async Task<ApiResponse<Doctor>> UpdateDoctorAsync(int id, DoctorDto dto, string webRootPath)
        {
            var doctor = await _repository.GetByIdAsync(id);

            if (doctor == null)
                return new ApiResponse<Doctor> { Success = false, Message = "Not found" };

            doctor.Name = dto.Name;
            doctor.Department = dto.Department;

            if (dto.Photo != null)
            {
                var uploads = Path.Combine(webRootPath, "uploads");

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Photo.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Photo.CopyToAsync(stream);

                doctor.PhotoUrl = "/uploads/" + fileName;
            }

            await _repository.SaveChangesAsync();

            var rooms = await _repository.GetDoctorRoomNumbersAsync(id);

            foreach (var room in rooms)
                await _displayService.PushRoomUpdate(room);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        public async Task<ApiResponse<object>> DeleteDoctorAsync(int id)
        {
            var doctor = await _repository.GetByIdAsync(id);

            if (doctor == null)
                return new ApiResponse<object> { Success = false, Message = "Not found" };

            await _repository.RemoveAsync(doctor);
            await _repository.SaveChangesAsync();

            var rooms = await _repository.GetDoctorRoomNumbersAsync(id);

            foreach (var room in rooms)
                await _displayService.PushRoomUpdate(room);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Doctor deleted"
            };
        }
    }
}