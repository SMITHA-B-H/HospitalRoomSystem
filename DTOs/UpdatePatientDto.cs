namespace HospitalRoomAPI.DTOs
{
    public class UpdatePatientDto
    {
        public string? Name { get; set; }

        public int Age { get; set; }

        public int? DoctorId { get; set; }

        public string? PatientType { get; set; }
    }
}