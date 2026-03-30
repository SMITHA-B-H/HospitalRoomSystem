using System.Collections.Generic;

namespace HospitalRoomAPI.Models
{
    public class Hospital
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LogoUrl { get; set; }

        public string Address { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Doctor> Doctors { get; set; }

        public ICollection<Floor> Floors { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
}