using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventRegistrationSystem.Repositories;
using System.Security.Claims;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class RegistrationsController : Controller
    {
        private readonly IRegistrationRepository _regRepo;
        private readonly IEventRepository _eventRepo;

        public RegistrationsController(
            IRegistrationRepository regRepo,
            IEventRepository eventRepo)
        {
            _regRepo = regRepo;
            _eventRepo = eventRepo;
        }

        // GET: Admin/Registrations/Index
        public async Task<IActionResult> Index(
    int? eventId,
    string? search,
    bool? checkedIn,
    int page = 1)
        {
            int pageSize = 10;

            var pagedResult = await _regRepo.GetPagedDtoAsync(
                page: page,
                pageSize: pageSize,
                eventId: eventId,
                search: search,
                checkedIn: checkedIn
            );

            // Overall event totals (ignore search for stats)
            int total = eventId.HasValue ? await _regRepo.GetCountByEventAsync(eventId.Value) : 0;
            int checkedInCount = eventId.HasValue ? await _regRepo.GetFilteredCheckedInCountAsync(eventId, search: null) : 0;
            double percentage = total > 0 ? Math.Round((double)checkedInCount / total * 100, 1) : 0;

            ViewBag.TotalRegistrations = total;
            ViewBag.CheckedInCount = checkedInCount;
            ViewBag.Percentage = percentage;
            ViewBag.EventId = eventId;

            if (eventId.HasValue)
            {
                var ev = await _eventRepo.GetByIdAsync(eventId.Value);
                ViewBag.EventTitle = ev?.Title;
            }

            return View("~/Views/Admin/Registrations/Index.cshtml", pagedResult);
        }

        // GET: Admin/Registrations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var registration = await _regRepo.GetByIdAsync(id);
            if (registration == null) return NotFound();

            // 👇 EXPLICIT VIEW PATH
            return View("~/Views/Admin/Registrations/Details.cshtml", registration);
        }

        // POST: Admin/Registrations/ToggleCheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken] // 👈 Added for security
        public async Task<IActionResult> ToggleCheckIn(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _regRepo.ToggleCheckInAsync(id, userId!);
            return Json(new { success });
        }

        // POST: Admin/Registrations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? eventId, string? search, bool? checkedIn, int page = 1)
        {
            await _regRepo.DeleteAsync(id);
            TempData["Success"] = "Registration deleted successfully.";
            return RedirectToAction(nameof(Index), new { eventId, search, checkedIn, page });
        }

        // GET: Admin/Registrations/Export
        public async Task<IActionResult> Export(int? eventId, string? search, bool? checkedIn)
        {
            var registrations = await _regRepo.GetAllAsync(eventId, search, checkedIn);
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Event,Attendee,Email,Registered,Status");

            foreach (var r in registrations)
            {
                csv.AppendLine($"{r.Event?.Title},{r.User?.FullName},{r.User?.Email},{r.RegisteredAt:yyyy-MM-dd HH:mm},{(r.IsCheckedIn ? "Checked in" : "Not checked in")}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"registrations_{DateTime.Now:yyyyMMdd}.csv");
        }
        // GET: Admin/Registrations/ExportByEvent/5
        public async Task<IActionResult> ExportByEvent(int eventId)
        {
            var registrations = await _regRepo.GetAllAsync(eventId: eventId, search: null, checkedIn: null);
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Event,Attendee,Email,Registered,Status,TicketType");

            foreach (var r in registrations)
            {
                csv.AppendLine($"\"{r.Event?.Title}\",\"{r.User?.FullName}\",\"{r.User?.Email}\",{r.RegisteredAt:yyyy-MM-dd HH:mm},{(r.IsCheckedIn ? "Checked in" : "Not checked in")},\"{r.TicketType}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var eventName = registrations.FirstOrDefault()?.Event?.Title?.Replace(" ", "_") ?? "Event";
            return File(bytes, "text/csv", $"registrations_{eventName}_{DateTime.Now:yyyyMMdd}.csv");
        }
        public async Task<IActionResult> EventOverview(int page = 1)
        {
            int pageSize = 6;
            var pagedResult = await _eventRepo.GetPagedEventSummariesAsync(page, pageSize);
            return View("~/Views/Admin/Registrations/EventOverview.cshtml", pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> ExportAllForEvent(int eventId)
        {
            var registrations = await _regRepo.GetAllAsync(eventId: eventId, search: null, checkedIn: null);
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Event,Attendee,Email,Registered,Status,TicketType");

            foreach (var r in registrations)
            {
                csv.AppendLine($"\"{r.Event?.Title}\",\"{r.User?.FullName}\",\"{r.User?.Email}\",{r.RegisteredAt:yyyy-MM-dd HH:mm},{(r.IsCheckedIn ? "Checked in" : "Not checked in")},\"{r.TicketType}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var eventName = registrations.FirstOrDefault()?.Event?.Title?.Replace(" ", "_") ?? "Event";
            return File(bytes, "text/csv", $"registrations_{eventName}_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}