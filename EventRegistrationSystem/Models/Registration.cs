using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventRegistrationSystem.Models
{
    public class Registration
    {
        public int Id { get; set; }
        
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public Event? Event { get; set; }
        
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        
        public string? TicketType { get; set; }
        
        public bool IsCheckedIn { get; set; } = false;
        
        [DataType(DataType.DateTime)]
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
    }
}