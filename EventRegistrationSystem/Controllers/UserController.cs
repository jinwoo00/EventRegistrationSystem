using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.ViewModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.Controllers
{
    [Authorize] // User must be logged in
    public class UserController : Controller
    {
        private readonly IEventRepository _eventRepo;
        private readonly IRegistrationRepository _regRepo;
        private readonly ICertificateRepository _certRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public UserController(
            IEventRepository eventRepo,
            IRegistrationRepository regRepo,
            ICertificateRepository certRepo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _eventRepo = eventRepo;
            _regRepo = regRepo;
            _certRepo = certRepo;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /UserDashboard
        public async Task<IActionResult> Index()
        {
            var events = await _eventRepo.GetPublishedEventsAsync();
            var userId = _userManager.GetUserId(User);

            // Pass registered event IDs to the view to disable buttons
            var registeredEventIds = new List<int>();
            foreach (var ev in events)
            {
                if (await _regRepo.IsUserRegisteredAsync(ev.Id, userId))
                    registeredEventIds.Add(ev.Id);
            }

            ViewBag.RegisteredEventIds = registeredEventIds;
            return View(events);
        }

        // POST: /UserDashboard/Register/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var success = await _regRepo.RegisterUserForEventAsync(eventId, userId);
            if (success)
                TempData["Success"] = "Successfully registered for the event!";
            else
                TempData["Error"] = "You are already registered for this event.";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MyTickets()
        {
            var userId = _userManager.GetUserId(User);
            var registrations = await _regRepo.GetUserRegistrationsAsync(userId);
            return View(registrations);
        }
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var registrations = await _regRepo.GetUserRegistrationsAsync(userId);

            var now = DateTime.Now;
            var upcoming = registrations
                .Where(r => r.EventStartDate > now)
                .OrderBy(r => r.EventStartDate)
                .ToList();
            var past = registrations
                .Where(r => r.EventStartDate <= now && r.IsCheckedIn)
                .OrderByDescending(r => r.EventStartDate)
                .ToList();

            var viewModel = new UserDashboardViewModel
            {
                UserName = User.Identity?.Name ?? "User",
                UpcomingEvents = upcoming.Take(3).ToList(),
                PastEvents = past.Take(3).ToList(),
                TotalEvents = registrations.Count,
                AttendedCount = registrations.Count(r => r.IsCheckedIn),
                UpcomingCount = upcoming.Count,
                PastCount = past.Count,
                CertificatesCount = await _certRepo.GetUserCertificateCountAsync(userId) // if available
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Certificates()
        {
            var userId = _userManager.GetUserId(User);
            var certificates = await _certRepo.GetUserCertificatesAsync(userId);
            // Assuming GetUserCertificatesAsync already returns only approved or we filter here
            return View(certificates.Where(c => c.IsApproved).ToList());
        }
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            var userId = _userManager.GetUserId(User);
            var cert = await _certRepo.GetByIdAsync(id);
            if (cert == null || cert.UserId != userId)
                return NotFound();

            if (string.IsNullOrEmpty(cert.FilePath))
                return NotFound("Certificate file not found.");

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, cert.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("Certificate file is missing.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/pdf", $"certificate-{cert.CertificateNumber}.pdf");
        }
    }
}