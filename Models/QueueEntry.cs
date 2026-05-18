using System.ComponentModel.DataAnnotations;

namespace HospitalRoomAPI.Models
{
    public class QueueEntry
    {
        public int Id { get; set; }

        public int TokenNumber { get; set; }

        [Required]
        public string PatientName { get; set; } = "";

        [Required]
        public string Department { get; set; } = "";

        public int DoctorId { get; set; }

        // NAVIGATION
        public Doctor? Doctor { get; set; }

        // OPD / LAB / SCAN / XRAY / PHARMACY
        public string Stage { get; set; } = "OPD";

        // Waiting / InProgress / Completed / Skipped
        public string Status { get; set; } = "Waiting";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DisplayNumber { get; set; } = "";
    }
}