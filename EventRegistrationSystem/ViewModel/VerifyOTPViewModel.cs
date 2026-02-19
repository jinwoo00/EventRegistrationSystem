using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.ViewModel
{
    public class VerifyOTPViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string OTP { get; set; } = string.Empty;

        public bool CanResend { get; set; } = false;
    }
}