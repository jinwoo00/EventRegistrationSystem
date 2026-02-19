using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
    public class ParticipantCertificateDTO
    {
        public int RegistrationId { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public DateTime CheckedInAt { get; set; }
        public bool HasCertificate { get; set; }
        public DateTime? CertificateSentAt { get; set; }
    }
}
