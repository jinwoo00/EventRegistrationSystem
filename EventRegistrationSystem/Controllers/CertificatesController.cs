using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateRepository _certRepo;
        private readonly IEventRepository _eventRepo;

        public CertificatesController(
            ICertificateRepository certRepo,
            IEventRepository eventRepo)
        {
            _certRepo = certRepo;
            _eventRepo = eventRepo;
        }

        // GET: /Admin/Certificates/Index
        public async Task<IActionResult> Index(int? eventId, string? search, int page = 1)
        {
            if (!eventId.HasValue)
                return RedirectToAction(nameof(EventOverview));

            int pageSize = 10;
            var pagedResult = await _certRepo.GetPagedAsync(
                eventId: eventId,
                search: search,
                page: page,
                pageSize: pageSize
            );

            // Overall event stats (ignore search)
            int totalIssued = await _certRepo.GetTotalIssuedAsync(eventId);
            int pending = await _certRepo.GetPendingCountAsync(eventId);
            int issuedToday = await _certRepo.GetIssuedTodayAsync(eventId);

            ViewBag.TotalIssued = totalIssued;
            ViewBag.Pending = pending;
            ViewBag.IssuedToday = issuedToday;
            ViewBag.EventId = eventId;

            var ev = await _eventRepo.GetByIdAsync(eventId.Value);
            ViewBag.EventTitle = ev?.Title;

            return View("~/Views/Admin/Certificates/Index.cshtml", pagedResult);
        }

        // GET: /Admin/Certificates/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var cert = await _certRepo.GetByIdAsync(id);
            if (cert == null || string.IsNullOrEmpty(cert.FilePath))
                return NotFound();

            // Build the full physical path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cert.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                // Fallback: if file not found, return a placeholder message
                return Content("Certificate file not found. (Integration pending)");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/pdf", $"certificate-{cert.CertificateNumber}.pdf");
        }

        // POST: /Admin/Certificates/Revoke/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int id, int? eventId)
        {
            var success = await _certRepo.RevokeAsync(id);
            if (success)
                TempData["Success"] = "Certificate revoked successfully.";
            else
                TempData["Error"] = "Certificate not found.";

            return RedirectToAction(nameof(Index), new { eventId });
        }
        public async Task<IActionResult> EventOverview(int page = 1)
        {
            int pageSize = 6;
            var pagedResult = await _eventRepo.GetPagedEventSummariesAsync(page, pageSize);
            return View("~/Views/Admin/Certificates/EventOverview.cshtml", pagedResult);
        }
    }
}