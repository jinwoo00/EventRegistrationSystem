using EventRegistrationSystem.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.ViewModel.User
{
    public class UserDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<UserRegistrationDTO> UpcomingEvents { get; set; } = new();
        public List<UserRegistrationDTO> PastEvents { get; set; } = new();
        public int TotalEvents { get; set; }
        public int AttendedCount { get; set; }
        public int UpcomingCount { get; set; }      // new
        public int PastCount { get; set; }           // new
        public int CertificatesCount { get; set; }
    }
}
