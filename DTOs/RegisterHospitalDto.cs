namespace HospitalRoomAPI.DTOs
{
    public class RegisterHospitalDto
    {
        public string HospitalName { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Address { get; set; }   // ? add this
    }
}