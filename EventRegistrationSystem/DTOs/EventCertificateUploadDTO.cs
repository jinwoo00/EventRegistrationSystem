using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
        public class EventCertificateUploadDto
        {
            public int EventId { get; set; }
            public string EventTitle { get; set; } = string.Empty;
            public bool HasTemplate { get; set; }
            public string? TemplateFileName { get; set; }
            public DateTime? TemplateUploadedAt { get; set; }
        }
    
}
