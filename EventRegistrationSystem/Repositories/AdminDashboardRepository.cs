using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using Microsoft.EntityFrameworkCore;
using static EventRegistrationSystem.DTOs.AdminDashboardDTO;

namespace EventRegistrationSystem.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfDay = now.Date;

            // Total Events
            var totalEvents = await _context.Events.CountAsync();
            var newEventsThisMonth = await _context.Events
                .CountAsync(e => e.CreatedAt >= startOfMonth);

            // Total Registrations
            var totalRegistrations = await _context.Registrations.CountAsync();
            var newRegistrationsToday = await _context.Registrations
                .CountAsync(r => r.RegisteredAt >= startOfDay);

            // Check-ins (Attendance)
            var totalCheckedIn = await _context.Attendances
                .CountAsync(a => a.CheckedInAt != null);

            var totalAttendees = await _context.Registrations.CountAsync();
            var checkInPercentage = totalAttendees > 0
                ? Math.Round((double)totalCheckedIn / totalAttendees * 100, 1)
                : 0;

            // Certificates
            var certificatesGenerated = await _context.Certificates.CountAsync();

            return new DashboardStats
            {
                TotalEvents = totalEvents,
                NewEventsThisMonth = newEventsThisMonth,
                TotalRegistrations = totalRegistrations,
                NewRegistrationsToday = newRegistrationsToday,
                TotalCheckedIn = totalCheckedIn,
                TotalAttendees = totalAttendees,
                CheckInPercentage = checkInPercentage,
                CertificatesGenerated = certificatesGenerated
            };
        }
        // Inside AdminDashboardRepository class
        public async Task<List<RegistrationTrendDto>> GetRegistrationTrendAsync(int days = 30)
        {
            var startDate = DateTime.Today.AddDays(-days + 1);
            var registrations = await _context.Registrations
                .Where(r => r.RegisteredAt >= startDate)
                .GroupBy(r => r.RegisteredAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new List<RegistrationTrendDto>();
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                var count = registrations.FirstOrDefault(r => r.Date == date)?.Count ?? 0;
                result.Add(new RegistrationTrendDto
                {
                    Date = date.ToString("MMM dd"),
                    Count = count
                });
            }
            return result.OrderBy(r => r.Date).ToList();
        }

        public async Task<List<EventAttendanceRateDto>> GetEventAttendanceRatesAsync(int top = 10)
        {
            var events = await _context.Events
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    Registered = e.Registrations!.Count,
                    CheckedIn = _context.Attendances
                        .Count(a => a.Registration != null
                                 && a.Registration.EventId == e.Id
                                 && a.CheckedInAt != null)
                })
                .OrderByDescending(e => e.Registered)
                .Take(top)
                .ToListAsync();

            return events.Select(e => new EventAttendanceRateDto
            {
                EventName = e.Title,
                Registered = e.Registered,
                CheckedIn = e.CheckedIn,
                AttendancePercentage = e.Registered == 0 ? 0 : Math.Round((double)e.CheckedIn / e.Registered * 100, 1)
            }).ToList();
        }

        public async Task<AttendanceStatusDto> GetOverallAttendanceStatusAsync()
        {
            var totalReg = await _context.Registrations.CountAsync();
            var checkedIn = await _context.Attendances.CountAsync(a => a.CheckedInAt != null);
            var noShow = totalReg - checkedIn;

            return new AttendanceStatusDto
            {
                TotalRegistrations = totalReg,
                CheckedIn = checkedIn,
                NoShow = noShow
            };
        }
    }
}