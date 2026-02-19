using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrationSystem.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly IRegistrationRepository _regRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StaffController> _logger;

        public StaffController(
            IRegistrationRepository regRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<StaffController> logger = null)
        {
            _regRepo = regRepo;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("Staff/ProcessScan/{id}")]
        public async Task<IActionResult> ProcessScan(int id)
        {
            try
            {
                var reg = await _regRepo.GetByIdAsync(id);
                if (reg == null)
                    return Json(new { success = false, message = "Invalid QR code." });

                var userId = _userManager.GetUserId(User);
                if (!reg.IsCheckedIn)
                {
                    var success = await _regRepo.CheckInAsync(id, userId!);
                    var message = success ? $"{reg.User?.FullName ?? reg.User?.Email} checked in." : "Check‑in failed.";
                    return Json(new { success, message });
                }
                else if (reg.IsCheckedIn && reg.CheckedOutAt == null)
                {
                    var success = await _regRepo.CheckOutAsync(id, userId!);
                    var message = success ? $"{reg.User?.FullName ?? reg.User?.Email} checked out." : "Check‑out failed.";
                    return Json(new { success, message });
                }
                else
                {
                    return Json(new { success = false, message = "Attendee already checked out." });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ProcessScan error");
                return Json(new { success = false, message = "Server error." });
            }
        }

        [HttpGet("Staff/Scan")]
        public IActionResult Scan() => View();

        [HttpGet("Staff/Attendance")]
        public async Task<IActionResult> Attendance(int? eventId, string? search, int page = 1)
        {
            var pagedResult = await _regRepo.GetAttendanceListAsync(eventId, search, null, page, 10);
            return View(pagedResult);
        }

        [HttpPost("Staff/CheckIn/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckInAsync(id, userId!);
            return Json(new { success });
        }

        [HttpPost("Staff/CheckOut/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckOutAsync(id, userId!);
            return Json(new { success });
        }
    }
}