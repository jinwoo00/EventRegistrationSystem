using System.ComponentModel.DataAnnotations;

namespace EventRegistrationSystem.ViewModel.Admin
{
    public class EventViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        public string? Location { get; set; }

        [Range(1, 10000)]
        public int? Capacity { get; set; }

        public bool IsPublished { get; set; } = true;

        public IFormFile? PosterFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        // 👇 Default values for new events
        public EventViewModel()
        {
            StartDate = DateTime.Today.AddDays(1).AddHours(9);   // tomorrow 09:00
            EndDate = StartDate.AddHours(2);                    // +2 hours
        }

        // 👇 Format for datetime-local input (no seconds)
        public string StartDateFormatted => StartDate.ToString("yyyy-MM-ddTHH:mm");
        public string EndDateFormatted => EndDate.ToString("yyyy-MM-ddTHH:mm");
    }
}