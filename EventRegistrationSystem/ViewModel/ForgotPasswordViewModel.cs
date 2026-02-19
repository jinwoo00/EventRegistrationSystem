using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid email address (e.g., name@domain.com).")]
        public string Email { get; set; } = string.Empty;
    }
}