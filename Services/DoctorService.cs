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
        private readonly IFileStorageService _fileService;

        public DoctorService(
            IDoctorRepository repository,
            IDisplayService displayService,
            IFileStorageService fileService)
        {
            _repository = repository;
            _displayService = displayService;
            _fileService = fileService;
        }

        // ================= GET =================
        public async Task<ApiResponse<List<Doctor>>> GetDoctorsAsync(int hospitalId)
        {
            var doctors = await _repository.GetDoctorsAsync(hospitalId);

            return new ApiResponse<List<Doctor>>
            {
                Success = true,
                Data = doctors
            };
        }

        // ================= CREATE =================
        public async Task<ApiResponse<Doctor>> AddDoctorAsync(
            DoctorDto dto,
            int hospitalId,
            string role)
        {
            if (role != "SuperAdmin")
                return new ApiResponse<Doctor> { Success = false, Message = "Forbidden" };

            string? photoUrl = null;

            if (dto.Photo != null)
            {
                photoUrl = await _fileService.UploadAsync(dto.Photo);
            }

            var doctor = new Doctor
            {
                Name = dto.Name,
                Department = dto.Department,
                PhotoUrl = photoUrl,
                HospitalId = hospitalId
            };

            await _repository.AddAsync(doctor);
            await _repository.SaveChangesAsync();

            await PushUpdates(doctor.Id);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        // ================= UPDATE =================
        public async Task<ApiResponse<Doctor>> UpdateDoctorAsync(int id, DoctorDto dto)
        {
            var doctor = await _repository.GetByIdAsync(id);

            if (doctor == null)
                return new ApiResponse<Doctor> { Success = false, Message = "Not found" };

            doctor.Name = dto.Name;
            doctor.Department = dto.Department;

            // ?? IMPORTANT FIX
            if (dto.Photo != null)
            {
                // OPTIONAL: delete old file
                if (!string.IsNullOrEmpty(doctor.PhotoUrl))
                {
                    var oldFileId = ExtractFileId(doctor.PhotoUrl);
                    await _fileService.DeleteAsync(oldFileId);
                }

                var newUrl = await _fileService.UploadAsync(dto.Photo);

                // ? FORCE UPDATE
                doctor.PhotoUrl = newUrl;
            }

            await _repository.SaveChangesAsync();

            await PushUpdates(id);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        // ================= DELETE =================
        public async Task<ApiResponse<Doctor>> DeleteDoctorAsync(int id)
        {
            var doctor = await _repository.GetByIdAsync(id);

            if (doctor == null)
                return new ApiResponse<Doctor>
                {
                    Success = false,
                    Message = "Not found"
                };

            // ?? DELETE FILE FROM DRIVE
            if (!string.IsNullOrEmpty(doctor.PhotoUrl))
            {
                var fileId = ExtractFileId(doctor.PhotoUrl);
                await _fileService.DeleteAsync(fileId);
            }

            await _repository.RemoveAsync(doctor);
            await _repository.SaveChangesAsync();

            await PushUpdates(id);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        // ================= COMMON =================
        private async Task PushUpdates(int doctorId)
        {
            var rooms = await _repository.GetDoctorRoomNumbersAsync(doctorId);

            var tasks = rooms
                .Where(r => !string.IsNullOrEmpty(r))
                .Select(r => _displayService.PushRoomUpdate(r!));

            await Task.WhenAll(tasks);
        }

        // ================= HELPER =================
        private string ExtractFileId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["id"];
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}