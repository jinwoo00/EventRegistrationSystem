namespace EventRegistrationSystem.DTOs
{
    public class EventCheckedInParticipantDTO
    {
        public int RegistrationId { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public DateTime CheckedInAt { get; set; }
        public bool HasCertificate { get; set; }
        public DateTime? CertificateSentAt { get; set; }
    }
}