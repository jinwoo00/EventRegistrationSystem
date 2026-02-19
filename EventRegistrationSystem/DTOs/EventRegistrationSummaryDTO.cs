namespace EventRegistrationSystem.DTOs
{
    public class EventRegistrationSummaryDTO
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public string? EventLocation { get; set; }
        public int TotalRegistrations { get; set; }
        public int CheckedInCount { get; set; }
        public double CheckInPercentage { get; set; }
        public string? ImageUrl { get; set; }
        public int CertificatesIssued { get; set; }
    }
}