namespace EventRegistrationSystem.DTOs
{
    public class RegistrationListItemDTO
    {
        public int Id { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public string? EventTitle { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string? TicketType { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }
}