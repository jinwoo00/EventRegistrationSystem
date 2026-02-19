using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventRegistrationSystem.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }
        public Event? Event { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [StringLength(100)]
        public string? CertificateNumber { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.Now;

        public string? FilePath { get; set; }
        public DateTime? SentAt { get; set; }
        public string? SentToEmail { get; set; }
        public bool IsTemplateBased { get; set; } = true;
        public bool IsApproved { get; set; } = false;
    }
}