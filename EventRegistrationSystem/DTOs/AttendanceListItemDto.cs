using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
    public class AttendanceListItemDto
    {
        public int RegistrationId { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public string? EventTitle { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public string? TicketType { get; set; }
    }
}
