using Microsoft.AspNetCore.Identity;

namespace EventRegistrationSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? EmailOTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? ProfilePictureUrl { get; set; }
    }
}
