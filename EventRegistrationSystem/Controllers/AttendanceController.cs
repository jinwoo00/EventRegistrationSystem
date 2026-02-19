using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class AttendanceController : Controller
    {
        private readonly IRegistrationRepository _regRepo;
        private readonly IEventRepository _eventRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(
            IRegistrationRepository regRepo,
            IEventRepository eventRepo,
            UserManager<ApplicationUser> userManager)
        {
            _regRepo = regRepo;
            _eventRepo = eventRepo;
            _userManager = userManager;
        }

        // GET: /Admin/Attendance/Index
        public async Task<IActionResult> Index(
    int? eventId,
    string? search,
    bool? checkedIn,
    int page = 1)
        {
            if (!eventId.HasValue)
                return RedirectToAction(nameof(EventOverview));

            int pageSize = 10;
            var pagedResult = await _regRepo.GetAttendanceListAsync(
                eventId: eventId,
                search: search,
                checkedIn: checkedIn,
                page: page,
                pageSize: pageSize
            );

            // Overall event totals (ignore search)
            var total = await _regRepo.GetCountByEventAsync(eventId.Value);
            var checkedInCount = await _regRepo.GetCheckedInCountAsync(eventId.Value);
            var checkedOutCount = await _regRepo.GetCheckedOutCountAsync(eventId.Value);
            var percentage = total > 0 ? Math.Round((double)checkedInCount / total * 100, 1) : 0;

            ViewBag.TotalRegistrations = total;
            ViewBag.CheckedInCount = checkedInCount;
            ViewBag.CheckedOutCount = checkedOutCount;
            ViewBag.CheckInPercentage = percentage;
            ViewBag.EventId = eventId;

            var ev = await _eventRepo.GetByIdAsync(eventId.Value);
            ViewBag.EventTitle = ev?.Title;

            return View("~/Views/Admin/Attendance/Index.cshtml", pagedResult);
        }

        // POST: /Admin/Attendance/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckInAsync(id, userId!);
            return Json(new { success });
        }

        // POST: /Admin/Attendance/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _regRepo.CheckOutAsync(id, userId!);
            return Json(new { success });
        }

        // GET: /Admin/Attendance/Scan
        public IActionResult Scan()
        {
            return View("~/Views/Admin/Attendance/Scan.cshtml");
        }

        // GET: /Admin/Attendance/ProcessScan/{id}
        // This is the URL embedded in QR codes
        public async Task<IActionResult> ProcessScan(int id)
        {
            var reg = await _regRepo.GetByIdAsync(id); // you need this method in repository
            if (reg == null)
                return NotFound();

            // Determine action: if not checked in, check in; if checked in and not checked out, check out
            if (!reg.IsCheckedIn)
            {
                var userId = _userManager.GetUserId(User);
                await _regRepo.CheckInAsync(id, userId!);
                TempData["Success"] = $"{reg.User?.FullName ?? reg.User?.Email} checked in successfully.";
            }
            else if (reg.IsCheckedIn && reg.CheckedOutAt == null)
            {
                var userId = _userManager.GetUserId(User);
                await _regRepo.CheckOutAsync(id, userId!);
                TempData["Success"] = $"{reg.User?.FullName ?? reg.User?.Email} checked out successfully.";
            }
            else
            {
                TempData["Error"] = "Attendee already checked out.";
            }

            // Redirect back to the attendance list (optionally with event filter)
            return RedirectToAction(nameof(Index), new { eventId = reg.EventId });
        }
        public async Task<IActionResult> EventOverview(int page = 1)
        {
            int pageSize = 6;
            var pagedResult = await _eventRepo.GetPagedEventSummariesAsync(page, pageSize);
            return View("~/Views/Admin/Attendance/EventOverview.cshtml", pagedResult);
        }
    }
}