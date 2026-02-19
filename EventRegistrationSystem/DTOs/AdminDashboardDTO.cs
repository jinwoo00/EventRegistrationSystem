using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.DTOs
{
    public class AdminDashboardDTO : Controller
    {
        public class DashboardStats
        {
            public int TotalEvents { get; set; }
            public int NewEventsThisMonth { get; set; }
            public int TotalRegistrations { get; set; }
            public int NewRegistrationsToday { get; set; }
            public int TotalCheckedIn { get; set; }
            public int TotalAttendees { get; set; } // for percentage
            public int CertificatesGenerated { get; set; }
            public double CheckInPercentage { get; internal set; }
        }
        public class RegistrationTrendDto
        {
            public string Date { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public class EventAttendanceRateDto
        {
            public string EventName { get; set; } = string.Empty;
            public double AttendancePercentage { get; set; }
            public int Registered { get; set; }
            public int CheckedIn { get; set; }
        }

        public class AttendanceStatusDto
        {
            public int TotalRegistrations { get; set; }
            public int CheckedIn { get; set; }
            public int NoShow { get; set; }
            // Add CheckedOut if you have that data
        }
    }
}
