using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HospitalRoomAPI.Models
{
    
        public class Patient
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int Age { get; set; }

            // Doctor Relation
            public int DoctorId { get; set; }
            public Doctor Doctor { get; set; }

            // OPTIONAL reverse reference
            //public Bed? Bed { get; set; }
        }
    
}