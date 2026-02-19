using EventRegistrationSystem.Models;

public class CertificateTemplate
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public string UploadedBy { get; set; } = string.Empty;
}