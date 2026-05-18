namespace HospitalRoomAPI.DTOs
{
    public class SaveSettingsDto
    {
        public int? FloorId { get; set; }
        public int? RoomId { get; set; }

        public string ScrollingMessage { get; set; } = "";
        public int ScrollingSpeed { get; set; }  // ? NEW
        public int AdsVolume { get; set; } = 50;
        public bool ShowClock { get; set; } 
    }
}