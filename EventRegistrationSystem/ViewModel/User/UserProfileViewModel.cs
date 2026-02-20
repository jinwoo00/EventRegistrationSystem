using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.ViewModel
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Profile picture
        public IFormFile? ProfilePicture { get; set; }
        public string? ExistingProfilePictureUrl { get; set; }
    }
}