using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
    public class UserCertificateDTO
    {
        public int Id { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public DateTime IssuedAt { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string? FilePath { get; set; } // optional, for direct file link
        public bool IsApproved { get; set; }
    }
}
