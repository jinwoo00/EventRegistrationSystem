namespace EventRegistrationSystem.DTOs
{
    public class AuditLogListItemDTO
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}