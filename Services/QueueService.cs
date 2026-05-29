using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Hubs;
using Microsoft.AspNetCore.SignalR; 
using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Models.Common;
using System.Text;
using System.Text.Json;

namespace HospitalRoomAPI.Services
{
    public class QueueService : IQueueService
    {
        private readonly IQueueRepository _repo;
        private readonly IHubContext<RoomHub> _hub;
        private readonly IDoctorRepository _doctorRepository;

        public QueueService(
            IQueueRepository repo,
            IHubContext<RoomHub> hub,
            IDoctorRepository doctorRepository) 

        {
            _repo = repo;
            _hub = hub;
            _doctorRepository = doctorRepository;
        }

        // =====================================
        // GET ALL
        // =====================================
        public async Task<List<QueueEntry>>
            GetAllQueueAsync()
        {
            return await _repo
                .GetAllQueueAsync();
        }

        // =====================================
        // GET DOCTOR
        // =====================================
        public async Task<List<QueueEntry>>
            GetDoctorQueueAsync(
            int doctorId)
        {
            return await _repo
                .GetDoctorQueueAsync(
                    doctorId);
        }

        // =====================================
        // GET STAGE
        // =====================================
        public async Task<List<QueueEntry>>
            GetByStageAsync(
            string stage)
        {
            return await _repo
                .GetByStageAsync(
                    stage);
        }

        // =====================================
        // ADD PATIENT
        // =====================================
        public async Task<QueueEntry>
            AddToQueueAsync(
            QueueCreateDto dto)
        {
            var today =
                DateTime.Today;

            var lastToken =
                await _repo
                .GetLastActiveTokenByDoctorAsync(
                    dto.DoctorId,
                    today
                );

            var doctor = await _doctorRepository
             .GetByIdAsync(dto.DoctorId);

            var hospitalName =
                await _doctorRepository
                .GetHospitalNameAsync(
                    doctor.HospitalId
                );

            string stage = "OPD";

            var dept =
                dto.Department
                .ToLower();

            if (dept.Contains("lab"))
                stage = "LAB";

            else if (dept.Contains("scan"))
                stage = "SCAN";

            else if (dept.Contains("xray"))
                stage = "XRAY";

            var entry =
                new QueueEntry
                {
                    TokenNumber =
                        lastToken + 1,

                    PatientName =
                        dto.PatientName,
                    PhoneNumber =
                        dto.PhoneNumber,

                    Department =
                        dto.Department,

                    DoctorId =
                        dto.DoctorId,

                    Stage = stage,

                    Status =
                        "Waiting",

                    CreatedAt =
                        DateTime.Now,
                    DisplayNumber = dto.DisplayNumber
                };

            await _repo
                .AddAsync(entry);

            await _repo
                .SaveAsync();

            using var client = new HttpClient();

            var payload = new
            {
                phone = dto.PhoneNumber,

                message =
                    $"Confirmed! Hi {dto.PatientName}, your appointment with {doctor.Name} at {hospitalName} on {DateTime.Now:dd-MM-yyyy} is confirmed. Please wait for your turn. Thank you."

            };

            var json =
                System.Text.Json.JsonSerializer.Serialize(
                    payload
                );

            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            try
            {
                await client.PostAsync(
                    "http://localhost:3000/send",
                    content
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "WhatsApp API Error: " + ex.Message
                );
            }


            await Broadcast();

            return entry;
        }

        // =====================================
        // CALL NEXT
        // ONLY STATUS CHANGE
        // =====================================
        public async Task<QueueEntry?>
        CallNextAsync(int doctorId)
        {
            // =====================================
            // COMPLETE CURRENT INPROGRESS
            // =====================================

            var allQueue =
                await _repo.GetDoctorQueueAsync(doctorId);

            var current =
                allQueue.FirstOrDefault(
                    x => x.Status == "InProgress");

            if (current != null)
            {
                current.Status = "Completed";
            }

            // =====================================
            // GET NEXT WAITING
            // =====================================

            var next =
                allQueue.FirstOrDefault(
                    x => x.Status == "Waiting");

            if (next == null)
            {
                await _repo.SaveAsync();
                await Broadcast();

                return null;
            }

            // =====================================
            // MAKE NEXT ACTIVE
            // =====================================

            next.Status = "InProgress";

            await _repo.SaveAsync();

            await Broadcast();

            return next;
        }

        // =====================================
        // COMPLETE
        // =====================================
        public async Task<QueueEntry?>
            CompleteAsync(
            int id)
        {
            var item =
                await _repo
                .GetByIdAsync(id);

            if (item == null)
                return null;

            item.Status =
                "Completed";

            await _repo
                .SaveAsync();

            await Broadcast();

            return item;
        }

        // =====================================
        // SKIP
        // ONLY SKIPPED GOES LAST
        // =====================================
        public async Task<QueueEntry?>
            SkipAsync(
            int id)
        {
            var item =
                await _repo
                .GetByIdAsync(id);

            if (item == null)
                return null;

            item.Status =
                "Skipped";

            // move bottom
            item.CreatedAt =
                DateTime.Now;

            await _repo
                .SaveAsync();

            await Broadcast();

            return item;
        }

        // =====================================
        // MOVE TO LAB / SCAN / XRAY
        // OPD ENTRY MUST COMPLETE
        // =====================================
        public async Task<QueueEntry?>
            MoveStageAsync(
            int id,
            string stage)
        {
            var item =
                await _repo
                .GetByIdAsync(id);

            if (item == null)
                return null;

            item.Stage =
                stage;

            item.Status =
                "Completed";

            await _repo
                .SaveAsync();

            await Broadcast();

            return item;
        }

        // =====================================
        // RECALL
        // BRING TO TOP
        // =====================================
        public async Task<QueueEntry?>
         RecallAsync(int id)
        {
            var item =
                await _repo
                .GetByIdAsync(id);

            if (item == null)
                return null;

            // =====================================
            // ONLY SKIPPED CAN RECALL
            // =====================================

            if (item.Status != "Skipped")
                return null;

            item.Status = "Waiting";

            // bring near top
            item.CreatedAt =
                DateTime.Now.AddMinutes(-10);

            await _repo.SaveAsync();

            await Broadcast();

            return item;
        }

        // =====================================
        // RESET
        // =====================================
        public async Task<string>
            ResetQueueAsync()
        {
            await _repo
                .ResetQueueAsync();

            await Broadcast();

            return
                "Queue Reset Successfully";
        }

        // =====================================
        // SIGNALR
        // =====================================
        private async Task
            Broadcast()
        {
            var fullQueue =
                await _repo
                .GetAllQueueAsync();

            await _hub
                .Clients
                .All
                .SendAsync(
                    "QueueUpdated",
                    fullQueue
                );
        }

        // =====================================
        // DISPLAY QUEUE
        // =====================================
        public async Task<DisplayQueueDto?>GetDisplayQueueAsync(string displayNumber)
        {
            // =====================================
            // GET QUEUE
            // =====================================

            var data =
                await _repo
                .GetDisplayQueueAsync(
                    displayNumber);

            // =====================================
            // GET DOCTOR EVEN IF QUEUE EMPTY
            // =====================================

            Doctor? doctor = null;

            if (data.Any())
            {
                doctor = data.First().Doctor;
            }
            else
            {
                doctor = await _doctorRepository
                    .GetByDisplayNumberAsync(
                        displayNumber);
            }

            // =====================================
            // STILL RETURN OBJECT
            // =====================================

            return new DisplayQueueDto
            {
                DisplayNumber = displayNumber,

                Doctor = doctor == null
                    ? null
                    : new DoctorDisplayDto
                    {
                        Id = doctor.Id,

                        Name = doctor.Name,

                        Department = doctor.Department,

                        Role = doctor.Role,

                        PhotoUrl = doctor.PhotoUrl,

                        DisplayNumber =
                            doctor.DisplayNumber
                    },

                Queue = data.Select(x =>
                    new QueueItemDto
                    {
                        Id = x.Id,

                        TokenNumber =
                            x.TokenNumber,

                        PatientName =
                            x.PatientName,

                        Status =
                            x.Status,

                        Stage =
                            x.Stage
                    }).ToList()
            };
        }


        // =====================================
        //  CENTRAL DISPLAY 
        // =====================================
        public async Task<List<CentralDisplayDto>>
        GetCentralDisplayAsync()
            {
                return await _repo
                    .GetCentralDisplayAsync();
            }



        ///========================================
        public async Task<ApiResponse<object>>ResetDoctorQueueAsync(int doctorId)
        {
            // =====================================
            // RESET DATABASE
            // =====================================

            var result = await _repo
                .ResetDoctorQueueAsync(
                    doctorId
                );

            // =====================================
            // GET DOCTOR
            // =====================================

            var doctor = await _doctorRepository
                .GetByIdAsync(doctorId);

            // =====================================
            // UPDATE OPD DISPLAY
            // =====================================

            if (doctor != null &&
                !string.IsNullOrEmpty(
                    doctor.DisplayNumber))
            {
                await _hub
                    .Clients
                    .Group(
                        doctor.DisplayNumber
                    )
                    .SendAsync(
                        "QueueUpdated",
                        new object[] { }
                    );

                Console.WriteLine(
                    $"?? OPD QueueUpdated SENT -> {doctor.DisplayNumber}"
                );
            }

            // =====================================
            // UPDATE CENTRAL DISPLAY
            // =====================================

            await _hub
                .Clients
                .Group("central")
                .SendAsync(
                    "QueueUpdated",
                    new object[] { }
                );

            Console.WriteLine(
                "?? CENTRAL QueueUpdated SENT"
            );

            return new ApiResponse<object>
            {
                Success = result.Success,
                Message = result.Message,
                Data = null
            };
        }

        //======================================

        public async Task<QueueEntry?>
        EmergencyCallAsync(int id)
        {
            var emergencyPatient =
                await _repo.GetByIdAsync(id);

            if (emergencyPatient == null)
                return null;

            // =====================================
            // FIND CURRENT ACTIVE PATIENT
            // =====================================

            var doctorQueue =
                await _repo.GetDoctorQueueAsync(
                    emergencyPatient.DoctorId
                );

            var current =
                doctorQueue.FirstOrDefault(
                    x => x.Status == "InProgress"
                );

            // =====================================
            // MOVE CURRENT BACK TO WAITING
            // =====================================

            if (current != null)
            {
                current.Status = "Waiting";
            }

            // =====================================
            // MAKE EMERGENCY ACTIVE
            // =====================================

            emergencyPatient.Status = "InProgress";

            // bring top
            emergencyPatient.CreatedAt =
                DateTime.Now.AddMinutes(-20);

            await _repo.SaveAsync();

            await Broadcast();

            return emergencyPatient;
        }

    }
}