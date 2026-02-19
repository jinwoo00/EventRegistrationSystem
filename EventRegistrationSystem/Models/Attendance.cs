using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventRegistrationSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [ForeignKey("Registration")]
        public int RegistrationId { get; set; }
        public Registration? Registration { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CheckedInAt { get; set; }

        public string? CheckedInBy { get; set; } // UserId of staff
        public DateTime? CheckedOutAt { get; set; }
        public string? CheckedOutBy { get; set; }
    }
}