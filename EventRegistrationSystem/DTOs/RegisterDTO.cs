using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.DTOs
{
    public class RegisterDTO
    {

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
    }
}
