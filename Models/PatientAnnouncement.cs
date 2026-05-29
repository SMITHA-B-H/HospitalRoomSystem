using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalRoomAPI.Models
{
    public class PatientAnnouncement
    {
        public int Id { get; set; }

        public int? PatientId { get; set; }

        public Patient? Patient { get; set; }

        public string? PatientName { get; set; }

        public string? Message { get; set; }

        public int? RoomId { get; set; }

        public int? FloorId { get; set; }

        public int? HospitalId { get; set; }

        public string? PosterUrl { get; set; }

        public string? VideoUrl { get; set; }

        public bool IsActive { get; set; }

        // Auto expiry
        public int ExpiryHours { get; set; }

        public DateTime? ExpiryTime { get; set; }

        // Add this
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}