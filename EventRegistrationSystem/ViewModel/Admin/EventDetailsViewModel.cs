using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.ViewModel.Admin
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = null!;
        public int TotalRegistrations { get; set; }
        public double CheckInPercentage { get; set; }
        public int CheckedInCount { get; set; }          // 👈 Add this
    }
}