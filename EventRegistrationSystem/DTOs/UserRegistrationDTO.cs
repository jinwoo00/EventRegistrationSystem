using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
    public class UserRegistrationDTO
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string? EventTitle { get; set; }
        public DateTime EventStartDate { get; set; }
        public string? EventLocation { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public string? EventImageUrl { get; set; }
    }
}
