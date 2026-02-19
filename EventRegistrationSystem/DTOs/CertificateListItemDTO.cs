namespace EventRegistrationSystem.DTOs
{
    public class CertificateListItemDTO
    {
        public int Id { get; set; }
        public string? EventTitle { get; set; }
        public string? AttendeeName { get; set; }
        public string? AttendeeEmail { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string? FilePath { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public string? SentToEmail { get; set; }
        public bool IsTemplateBased { get; set; }
    }
}