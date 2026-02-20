using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.ViewModel;

namespace EventRegistrationSystem.Controllers
{
    [Authorize(Roles = "Staff")]
    [Route("Staff")]
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

        // GET: /Staff/ProcessScan/{id}
        [HttpGet("ProcessScan/{id}")]
        public async Task<IActionResult> ProcessScan(int id)
        {
            var registration = await _regRepo.GetByIdAsync(id);
            if (registration == null)
            {
                TempData["ScanError"] = "Invalid QR code.";
                return RedirectToAction(nameof(Scan));
            }

            var user = registration.User;
            var ev = registration.Event;

            var viewModel = new ScanConfirmationViewModel
            {
                RegistrationId = registration.Id,
                AttendeeName = user?.FullName ?? "Unknown",
                AttendeeEmail = user?.Email ?? "Unknown",
                EventTitle = ev?.Title ?? "Unknown Event",
                EventStartDate = ev?.StartDate ?? DateTime.MinValue,
                EventLocation = ev?.Location ?? "Unknown",
                RegisteredAt = registration.RegisteredAt,
                IsCheckedIn = registration.IsCheckedIn,
                CheckedInAt = registration.CheckedInAt,
                CheckedOutAt = registration.CheckedOutAt
            };

            return View("~/Views/Staff/ConfirmScan.cshtml", viewModel);
        }

        // POST: /Staff/Confirm
        [HttpPost("Confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, string action)
        {
            var registration = await _regRepo.GetByIdAsync(id);
            if (registration == null)
            {
                TempData["ScanError"] = "Registration not found.";
                return RedirectToAction(nameof(Scan));
            }

            var userId = _userManager.GetUserId(User);
            string message = "";
            bool success = false;

            if (action == "checkin")
            {
                if (!registration.IsCheckedIn)
                {
                    success = await _regRepo.CheckInAsync(id, userId!);
                    if (success)
                    {
                        // Reload registration to get updated timestamps
                        registration = await _regRepo.GetByIdAsync(id);
                        message = $"Checked in at {registration.CheckedInAt:hh:mm tt}.";
                    }
                    else
                    {
                        message = "Check-in failed.";
                    }
                }
                else
                {
                    message = "Already checked in.";
                }
            }
            else if (action == "checkout")
            {
                if (registration.IsCheckedIn && registration.CheckedOutAt == null)
                {
                    success = await _regRepo.CheckOutAsync(id, userId!);
                    if (success)
                    {
                        registration = await _regRepo.GetByIdAsync(id);
                        message = $"Checked out at {registration.CheckedOutAt:hh:mm tt}.";
                    }
                    else
                    {
                        message = "Check-out failed.";
                    }
                }
                else
                {
                    message = "Cannot check out (not checked in or already checked out).";
                }
            }
            else
            {
                message = "No action taken.";
            }

            // Build the view model again with updated data
            var viewModel = new ScanConfirmationViewModel
            {
                RegistrationId = registration.Id,
                AttendeeName = registration.User?.FullName ?? "Unknown",
                AttendeeEmail = registration.User?.Email ?? "Unknown",
                EventTitle = registration.Event?.Title ?? "Unknown Event",
                EventStartDate = registration.Event?.StartDate ?? DateTime.MinValue,
                EventLocation = registration.Event?.Location ?? "Unknown",
                RegisteredAt = registration.RegisteredAt,
                IsCheckedIn = registration.IsCheckedIn,
                CheckedInAt = registration.CheckedInAt,
                CheckedOutAt = registration.CheckedOutAt
            };

            ViewBag.SuccessMessage = success ? message : null;
            ViewBag.ErrorMessage = success ? null : message;

            return View("~/Views/Staff/ConfirmScan.cshtml", viewModel);
        }

        // GET: /Staff/Scan
        [HttpGet("Scan")]
        public IActionResult Scan()
        {
            return View();
        }

        // GET: /Staff/Attendance
        [HttpGet("Attendance")]
        public async Task<IActionResult> Attendance(int? eventId, string? search, int page = 1)
        {
            var pagedResult = await _regRepo.GetAttendanceListAsync(eventId, search, null, page, 10);
            return View(pagedResult);
        }

        // POST: /Staff/CheckIn/{id} (optional, for manual use)
        [HttpPost("CheckIn/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckInAsync(id, userId!);
            return Json(new { success });
        }

        // POST: /Staff/CheckOut/{id}
        [HttpPost("CheckOut/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckOutAsync(id, userId!);
            return Json(new { success });
        }
    }
}