using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        public string? Location { get; set; }

        public int? Capacity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ImageUrl { get; set; }   
        public bool IsPublished { get; set; }
        public ICollection<Certificate>? Certificates { get; set; }

        // Navigation property
        public ICollection<Registration>? Registrations { get; set; }
    }
}