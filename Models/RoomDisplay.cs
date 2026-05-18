using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Models
{
    public class RoomDisplay
    {
        public string roomNumber { get; set; }
        public string roomName { get; set; }
        public string logoUrl { get; set; }
        public string scrollingMessage { get; set; }
        public int scrollingSpeed { get; set; }
        public int adsVolume { get; set; }
        public bool showClock { get; set; }

        public List<BedDisplayDto> beds { get; set; } = new();
        public List<string> videos { get; set; } = new();
        public List<AnnouncementDisplayDto> announcements { get; set; }
    }

    
}