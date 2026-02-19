using System.ComponentModel.DataAnnotations;
using static EventRegistrationSystem.DTOs.AdminDashboardDTO;

namespace EventRegistrationSystem.ViewModel.Admin
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardViewModel()
        {
            RegistrationTrend = new List<RegistrationTrendDto>();
            EventAttendanceRates = new List<EventAttendanceRateDto>();
            AttendanceStatus = new AttendanceStatusDto();
        }

        // Total Events
        public int TotalEvents { get; set; }
        public int NewEventsThisMonth { get; set; }

        // Registrations
        public int TotalRegistrations { get; set; }
        public int NewRegistrationsToday { get; set; }

        // Check-ins
        public int TotalCheckedIn { get; set; }
        public double CheckInPercentage { get; set; }

        // Certificates
        public int CertificatesGenerated { get; set; }

        // Current User Info
        public string CurrentUserName { get; set; } = string.Empty;
        public string CurrentUserEmail { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentUserInitials { get; set; } = string.Empty;

        // Date & Time
        public DateTime CurrentDateTime { get; set; }
        public List<RegistrationTrendDto> RegistrationTrend { get; set; } = new();
        public List<EventAttendanceRateDto> EventAttendanceRates { get; set; } = new();
        public AttendanceStatusDto AttendanceStatus { get; set; } = new();

    }
}
