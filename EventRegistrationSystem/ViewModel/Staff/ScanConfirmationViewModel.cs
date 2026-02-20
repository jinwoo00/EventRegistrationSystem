namespace EventRegistrationSystem.ViewModel
{
    public class ScanConfirmationViewModel
    {
        public int RegistrationId { get; set; }
        public string AttendeeName { get; set; } = string.Empty;
        public string AttendeeEmail { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}