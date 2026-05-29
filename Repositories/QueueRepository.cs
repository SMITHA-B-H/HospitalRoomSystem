// ==========================================
// QueueRepository.cs
// KEEP COMPLETED PATIENTS UNTIL RESET
// ==========================================

using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;


namespace HospitalRoomAPI.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private readonly AppDbContext _context;

        public QueueRepository(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // ALL QUEUE
        // =====================================
        public async Task<List<QueueEntry>>
        GetDoctorQueueAsync(int doctorId)
        {
            return await _context.QueueEntries
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.IsActive)
                .Include(x => x.Doctor)
                .OrderBy(x => x.TokenNumber)
                .ToListAsync();
        }

      
        // =====================================
        // STAGE QUEUE
        // =====================================
        public async Task<List<QueueEntry>>
            GetByStageAsync(string stage)
        {
            return await _context.QueueEntries
                .Where(x => x.Stage == stage)

               .OrderBy(x =>
                     x.Status == "Skipped" ? 9999 : x.TokenNumber)

                .ThenBy(x => x.CreatedAt)
                .ToListAsync();
        }

        // =====================================
        // NEXT WAITING ONLY
        // =====================================
        public async Task<QueueEntry?>
            GetNextWaitingAsync(int doctorId)
        {
            return await _context.QueueEntries
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.Status == "Waiting")

                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // =====================================
        public async Task<QueueEntry?>
            GetByIdAsync(int id)
        {
            return await _context.QueueEntries
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // =====================================
        public async Task<int>
            GetLastTokenByDoctorAsync(
            int doctorId,
            DateTime date)
        {
            return await _context.QueueEntries
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.CreatedAt.Date == date.Date)

                .OrderByDescending(x => x.TokenNumber)
                .Select(x => x.TokenNumber)
                .FirstOrDefaultAsync();
        }

        // =====================================
        public async Task AddAsync(
            QueueEntry entry)
        {
            await _context.QueueEntries
                .AddAsync(entry);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // =====================================
        // RESET ONLY ADMIN
        // =====================================
        public async Task ResetQueueAsync()
        {
            var all =
                await _context.QueueEntries
                .ToListAsync();

            _context.QueueEntries
                .RemoveRange(all);

            await _context.SaveChangesAsync();
        }

       

        // =====================================
        public async Task<List<QueueEntry>>
        GetDisplayQueueAsync(
        string displayNumber)
        {
            return await _context.QueueEntries

                .Include(x => x.Doctor)

                .Where(x =>
                    x.Doctor != null &&
                    x.Doctor.DisplayNumber ==
                    displayNumber && x.IsActive)

                .OrderBy(x =>
                    x.Status == "Skipped" ? 9999 : x.TokenNumber)
                .ThenBy(x => x.CreatedAt)

                .ToListAsync();
        }


        //===============================CENTERAL DISPLAY=============================
        //===============================
        // CENTERAL DISPLAY
        //===============================

        public async Task<List<CentralDisplayDto>>
    GetCentralDisplayAsync()
        {
            var result = await _context.QueueEntries

                .Include(x => x.Doctor)

                .Where(x =>

                    x.IsActive &&

                    (

                        x.Status == "InProgress" ||

                        x.Status == "Waiting"
                    )
                )

                .OrderBy(x => x.DoctorId)

                .ThenBy(x => x.TokenNumber)

                .Select(x => new CentralDisplayDto
                {
                    DoctorId =
                        x.DoctorId,

                    DoctorName =

                        x.Doctor != null

                            ? x.Doctor.Name

                            : "",

                    // =====================================
                    // SPECIALIZATION
                    // =====================================

                    DoctorDepartment =

                        x.Doctor != null

                            ? x.Doctor.Department

                            : "",

                    // =====================================
                    // ROOM / OPD
                    // =====================================

                    RoomNumber =

                        x.Doctor != null

                            ? x.Doctor.DisplayNumber

                            : "",

                    // =====================================
                    // TOKEN
                    // =====================================

                    CurrentToken =
                        x.TokenNumber,

                    // =====================================
                    // PATIENT
                    // =====================================

                    PatientName =
                        x.PatientName ?? "",

                    // =====================================
                    // STATUS
                    // =====================================

                    Status =
                        x.Status ?? ""
                })

                .ToListAsync();

            return result;
        }

        //======================================
        public async Task<int> GetLastActiveTokenByDoctorAsync(int doctorId,DateTime today)
        {
            var last =
                await _context.QueueEntries
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.CreatedAt.Date == today &&
                    x.IsActive)
                .OrderByDescending(x => x.TokenNumber)
                .FirstOrDefaultAsync();

            return last?.TokenNumber ?? 0;
        }


        //=========  
        public async Task<ApiResponse<object>>
        ResetDoctorQueueAsync(int doctorId)
        {
            var today = DateTime.Today;

            var queue =
                await _context.QueueEntries
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.CreatedAt.Date == today &&
                    x.IsActive)
                .ToListAsync();

            foreach (var item in queue)
            {
                item.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Queue Reset Successfully"
            };
        }

        // =====================================
        // ALL QUEUE
        // =====================================
        public async Task<List<QueueEntry>>
        GetAllQueueAsync()
        {
            return await _context.QueueEntries
                .Where(x => x.IsActive)
                .Include(x => x.Doctor)
                .OrderBy(x => x.TokenNumber)
                .ToListAsync();
        }

        

    }
}