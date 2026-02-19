using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(256)]
        public string? UserEmail { get; set; }

        [MaxLength(50)]
        public string? UserRole { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation property (optional)
        public ApplicationUser? User { get; set; }
    }
}