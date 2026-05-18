namespace HospitalRoomAPI.Models
{
    public class DisplayDevice
    {
        public int Id { get; set; }

        public string DeviceId { get; set; }      // Unique per device
        public string DeviceName { get; set; }    // ICU-TV-101
        public string RoomNumber { get; set; }    // 101

        public string ConnectionId { get; set; }  // SignalR connection

        public bool IsOnline { get; set; }        // true/false

        public DateTime LastSeen { get; set; }    // last active time
    }
}