using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.ViewModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static EventRegistrationSystem.DTOs.AdminDashboardDTO;

namespace EventRegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardRepository _dashboardRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IAdminDashboardRepository dashboardRepo,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardRepo = dashboardRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Admin";

            // 1. Dashboard stats (cards)
            var stats = await _dashboardRepo.GetDashboardStatsAsync();

            // 2. 📈 Chart data – PASTE THESE THREE LINES HERE
            var registrationTrend = await _dashboardRepo.GetRegistrationTrendAsync(30);      // last 30 days
            var eventRates = await _dashboardRepo.GetEventAttendanceRatesAsync(10);          // top 10 events
            var attendanceStatus = await _dashboardRepo.GetOverallAttendanceStatusAsync();

            // 3. Build the view model
            var viewModel = new AdminDashboardViewModel
            {
                TotalEvents = stats.TotalEvents,
                NewEventsThisMonth = stats.NewEventsThisMonth,
                TotalRegistrations = stats.TotalRegistrations,
                NewRegistrationsToday = stats.NewRegistrationsToday,
                TotalCheckedIn = stats.TotalCheckedIn,
                CheckInPercentage = stats.CheckInPercentage,
                CertificatesGenerated = stats.CertificatesGenerated,
                CurrentUserName = user?.FullName ?? user?.UserName ?? "Admin",
                CurrentUserEmail = user?.Email ?? "admin@eventflow.io",
                CurrentUserRole = role,
                CurrentUserInitials = GetInitials(user?.FullName ?? user?.UserName ?? "Admin"),
                CurrentDateTime = DateTime.Now,

                // 4. ✨ Assign chart data to the view model
                RegistrationTrend = registrationTrend,
                EventAttendanceRates = eventRates,
                AttendanceStatus = attendanceStatus
            };

            viewModel.RegistrationTrend = registrationTrend ?? new List<RegistrationTrendDto>();
            viewModel.EventAttendanceRates = eventRates ?? new List<EventAttendanceRateDto>();
            viewModel.AttendanceStatus = attendanceStatus ?? new AttendanceStatusDto();

            return View(viewModel);
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "AD";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
        }
    }
}