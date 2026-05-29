using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.Repositories;
using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalRoomAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _repository;
        private readonly IDisplayService _displayService;
        private readonly IConfiguration _config;
        private readonly IHubContext<RoomHub> _hubContext;
        private readonly AppDbContext _context;

        public DoctorService(
            IDoctorRepository repository,
            IDisplayService displayService,
            IConfiguration config,
            IHubContext<RoomHub> hubContext,
            AppDbContext context)
        {
            _repository = repository;
            _displayService = displayService;
            _config = config;
            _hubContext = hubContext;
            _context = context;
        }

        private string GetRootPath() => _config["StoragePath"]!;

        private async Task<string> SaveDoctorImage(IFormFile file)
        {
            var folder = Path.Combine(GetRootPath(), "doctors");

            Directory.CreateDirectory(folder);

            var fileName =
                $"{Guid.NewGuid()}_{file.FileName}";

            var filePath =
                Path.Combine(folder, fileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create
                );

            await file.CopyToAsync(stream);

            return
                $"/files/doctors/{fileName}";
        }

        // =====================================
        // GET
        // =====================================

        public async Task<ApiResponse<List<Doctor>>>
        GetDoctorsAsync(int hospitalId)
        {
            var doctors =
                await _repository
                .GetDoctorsAsync(hospitalId);

            return new ApiResponse<List<Doctor>>
            {
                Success = true,
                Data = doctors
            };
        }

        // =====================================
        // CREATE
        // =====================================

        public async Task<ApiResponse<Doctor>>
        AddDoctorAsync(
            DoctorDto dto,
            int hospitalId,
            string role)
        {
            if (role != "SuperAdmin")
            {
                return new ApiResponse<Doctor>
                {
                    Success = false,
                    Message = "Forbidden"
                };
            }

            string? photoUrl = null;

            if (dto.Photo != null)
            {
                photoUrl =
                    await SaveDoctorImage(dto.Photo);
            }

            var doctor = new Doctor
            {
                EmployeeId = dto.EmployeeId,
                Name = dto.Name,
                Department = dto.Department,
                Role = dto.Role,
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

        // =====================================
        // UPDATE
        // =====================================

        public async Task<ApiResponse<Doctor>>
        UpdateDoctorAsync(
            int id,
            DoctorDto dto)
        {
            var doctor =
                await _repository.GetByIdAsync(id);

            if (doctor == null)
            {
                return new ApiResponse<Doctor>
                {
                    Success = false,
                    Message = "Not found"
                };
            }

            doctor.Name = dto.Name;

            doctor.Department =
                dto.Department;

            doctor.EmployeeId =
                dto.EmployeeId;

            doctor.Role = dto.Role;

            if (dto.Photo != null)
            {
                if (!string.IsNullOrEmpty(
                    doctor.PhotoUrl))
                {
                    var oldPath = Path.Combine(
                        GetRootPath(),
                        doctor.PhotoUrl.Replace(
                            "/files/",
                            ""
                        )
                    );

                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                doctor.PhotoUrl =
                    await SaveDoctorImage(dto.Photo);
            }

            await _repository.SaveChangesAsync();

            await PushUpdates(id);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor
            };
        }

        // =====================================
        // DELETE
        // =====================================

        public async Task<ApiResponse<Doctor>>
        DeleteDoctorAsync(int id)
        {
            var doctor =
                await _repository.GetByIdAsync(id);

            if (doctor == null)
            {
                return new ApiResponse<Doctor>
                {
                    Success = false,
                    Message = "Doctor not found"
                };
            }

            // =====================================
            // CHECK ACTIVE PATIENTS
            // =====================================

            bool hasPatients =
                await _repository
                .HasAssignedPatientsAsync(id);

            if (hasPatients)
            {
                return new ApiResponse<Doctor>
                {
                    Success = false,
                    Message =
                        "Doctor cannot be deleted because patients are assigned."
                };
            }

            // =====================================
            // DELETE QUEUE ENTRIES
            // =====================================

            var queueEntries =
                await _context.QueueEntries
                .Where(q => q.DoctorId == id)
                .ToListAsync();

            if (queueEntries.Any())
            {
                _context.QueueEntries
                    .RemoveRange(queueEntries);

                await _context.SaveChangesAsync();
            }

            // =====================================
            // DELETE PHOTO
            // =====================================

            if (!string.IsNullOrEmpty(
                doctor.PhotoUrl))
            {
                var fullPath = Path.Combine(
                    GetRootPath(),
                    doctor.PhotoUrl.Replace(
                        "/files/",
                        ""
                    )
                );

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            // =====================================
            // DELETE DOCTOR
            // =====================================

            await _repository.RemoveAsync(doctor);

            await _repository.SaveChangesAsync();

            await PushUpdates(id);

            return new ApiResponse<Doctor>
            {
                Success = true,
                Data = doctor,
                Message =
                    "Doctor deleted successfully"
            };
        }

        // =====================================
        // UPDATE DISPLAY
        // =====================================

        public async Task<bool>
        UpdateDisplayAsync(
            int id,
            string displayNumber)
        {
            return await _repository
                .UpdateDisplayAsync(
                    id,
                    displayNumber
                );
        }

        // =====================================
        // SIGNALR PUSH
        // =====================================
        private async Task PushUpdates(int doctorId)
        {
            try
            {
                if (_hubContext == null)
                    return;

                if (_hubContext.Clients == null)
                    return;

                await _hubContext.Clients.All.SendAsync(
                    "DoctorUpdated",
                    doctorId
                );
            }
            catch
            {
                // ignore for unit tests
            }
        }
    }
}