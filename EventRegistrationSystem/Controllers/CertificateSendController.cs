using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Services;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Certificates/Send")]
    public class CertificateSendController : Controller
    {
        private readonly ICertificateRepository _certRepo;
        private readonly IEventRepository _eventRepo;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CertificateSendController> _logger;

        public CertificateSendController(
            ICertificateRepository certRepo,
            IEventRepository eventRepo,
            IEmailService emailService,
            ILogger<CertificateSendController> logger,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _certRepo = certRepo;
            _eventRepo = eventRepo;
            _emailService = emailService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Certificates/Send/Event/5
        [HttpGet("Event/{eventId}")]
        public async Task<IActionResult> Event(int eventId, int page = 1)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId);
            if (ev == null) return NotFound();

            int pageSize = 10;
            var pagedParticipants = await _certRepo.GetPagedCheckedInParticipantsAsync(eventId, page, pageSize);

            var template = await _certRepo.GetTemplateByEventAsync(eventId);

            ViewBag.Event = ev;
            ViewBag.Template = template;
            ViewBag.TotalPending = await _certRepo.GetPendingCountAsync(eventId);
            ViewBag.HasTemplate = template != null;

            return View("~/Views/Admin/Certificates/SendEvent.cshtml", pagedParticipants);
        }

        // POST: UploadTemplate
        [HttpPost("UploadTemplate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadTemplate(int eventId, IFormFile certificateFile)
        {
            if (certificateFile == null || certificateFile.Length == 0)
            {
                TempData["Error"] = "Please select a PDF file.";
                return RedirectToAction(nameof(Event), new { eventId });
            }

            if (!certificateFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction(nameof(Event), new { eventId });
            }

            var userId = _userManager.GetUserId(User)!;
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "certificate-templates");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"event-{eventId}-{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var relativePath = $"/uploads/certificate-templates/{uniqueFileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await certificateFile.CopyToAsync(stream);
            }

            var success = await _certRepo.UploadTemplateAsync(eventId, certificateFile.FileName, relativePath, userId);
            TempData[success ? "Success" : "Error"] = success
                ? "Certificate template uploaded successfully."
                : "Failed to upload template.";

            return RedirectToAction(nameof(Event), new { eventId });
        }

        // POST: SendToParticipant
        [HttpPost("SendToParticipant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToParticipant(int registrationId, int eventId)
        {
            var registration = await _context.Registrations
                .Include(r => r.User)
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == registrationId);
            if (registration?.User == null)
            {
                TempData["Error"] = "Participant not found.";
                return RedirectToAction(nameof(Event), new { eventId });
            }

            var template = await _certRepo.GetTemplateByEventAsync(eventId);
            if (template == null)
            {
                TempData["Error"] = "No certificate template found for this event. Please upload one first.";
                return RedirectToAction(nameof(Event), new { eventId });
            }

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, template.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Certificate template file is missing.";
                return RedirectToAction(nameof(Event), new { eventId });
            }

            // Read file into memory stream
            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Send email – pass stream directly
            await _emailService.SendWithAttachmentAsync(
                registration.User.Email,
                $"Your Certificate for {registration.Event.Title}",
                $@"
    <h2>Certificate of Attendance</h2>
    <p>Dear {registration.User.FullName ?? registration.User.Email},</p>
    <p>Thank you for attending <strong>{registration.Event.Title}</strong>.</p>
    <p>Please find your certificate attached.</p>
    <br>
    <p>Best regards,<br>EventFlow Team</p>",
                memory,                                 // stream
                $"certificate-{registration.Event.Id}-{registration.User.Id}.pdf", // filename
                "application/pdf"                       // contentType
            );

            // Mark as sent
            await _certRepo.MarkCertificateSentAsync(registrationId, registration.User.Email);

            TempData["Success"] = $"Certificate sent to {registration.User.Email}.";
            return RedirectToAction(nameof(Event), new { eventId });
        }

        //// POST: SendBulk
        //[HttpPost("SendBulk")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendBulk(int eventId)
        //{
        //    var template = await _certRepo.GetTemplateByEventAsync(eventId);
        //    if (template == null)
        //    {
        //        TempData["Error"] = "No certificate template found for this event.";
        //        return RedirectToAction(nameof(Event), new { eventId });
        //    }

        //    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, template.FilePath.TrimStart('/'));
        //    if (!System.IO.File.Exists(fullPath))
        //    {
        //        TempData["Error"] = "Certificate template file is missing.";
        //        return RedirectToAction(nameof(Event), new { eventId });
        //    }

        //    // Delegate for sending email
        //    async Task SendEmailAsync(string email, string eventTitle, string attendeeName, Stream templateStream)
        //    {
        //        var stampedPdf = PdfStamper.StampName(fullPath, attendeeName, logger: _logger);
        //        // If you have stored coordinates in the event, use them:
        //        // var stampedPdf = PdfStamper.StampName(fullPath, attendeeName, ev.CertificateX, ev.CertificateY, _logger);

        //        await _emailService.SendWithAttachmentAsync(
        //            email,
        //            $"Your Certificate for {eventTitle}",
        //            $@"
        //<h2>Certificate of Attendance</h2>
        //<p>Dear {attendeeName},</p>
        //<p>Thank you for attending <strong>{eventTitle}</strong>.</p>
        //<p>Please find your personalized certificate attached.</p>",
        //            stampedPdf,
        //            $"certificate-{attendeeName}.pdf",
        //            "application/pdf"
        //        );
        //    }

        //    var sentCount = await _certRepo.SendBulkCertificatesAsync(eventId, fullPath, SendEmailAsync);

        //    TempData["Success"] = $"{sentCount} certificate(s) sent successfully.";
        //    return RedirectToAction(nameof(Event), new { eventId });
        //}

        // POST: DeleteTemplate
        [HttpPost("DeleteTemplate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int eventId)
        {
            await _certRepo.DeleteTemplateAsync(eventId);
            TempData["Success"] = "Certificate template deleted.";
            return RedirectToAction(nameof(Event), new { eventId });
        }
        [HttpGet("Pending/{eventId}")]
        public async Task<IActionResult> Pending(int eventId)
        {
            var pending = await _certRepo.GetPendingCertificatesAsync(eventId);
            ViewBag.EventId = eventId;
            return View("~/Views/Admin/Certificates/Pending.cshtml", pending);
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int registrationId, int eventId)
        {
            try
            {
                // Get registration details
                var registration = await _context.Registrations
                    .Include(r => r.User)
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(r => r.Id == registrationId);
                if (registration?.User == null || registration.Event == null)
                {
                    TempData["Error"] = "Registration not found.";
                    return RedirectToAction(nameof(Event), new { eventId });
                }

                // Generate and save certificate
                var certNumber = $"CERT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20);
                var pdfBytes = CertificateGenerator.GenerateCertificate(
                    registration.User.FullName ?? registration.User.Email,
                    registration.Event.Title,
                    registration.Event.StartDate,
                    certNumber
                );

                // Save to wwwroot/certificates
                var certDir = Path.Combine(_webHostEnvironment.WebRootPath, "certificates");
                Directory.CreateDirectory(certDir);
                var fileName = $"cert-{certNumber}.pdf";
                var filePath = Path.Combine(certDir, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                // Create certificate record
                var certificate = new Certificate
                {
                    EventId = registration.EventId,
                    UserId = registration.UserId,
                    CertificateNumber = certNumber,
                    IssuedAt = DateTime.Now,
                    FilePath = $"/certificates/{fileName}",
                    IsTemplateBased = false,
                    SentAt = DateTime.Now,
                    SentToEmail = registration.User.Email,
                    IsApproved = true
                };
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Certificate approved for {registration.User.FullName ?? registration.User.Email}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving certificate");
                TempData["Error"] = "Failed to approve certificate.";
            }
            return RedirectToAction(nameof(Event), new { eventId });
        }

        [HttpPost("ApproveAll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAll(int eventId)
        {
            var participants = await _certRepo.GetCheckedInParticipantsAsync(eventId);
            var pending = participants.Where(p => !p.HasCertificate).ToList();

            int approved = 0;
            foreach (var p in pending)
            {
                var registration = await _context.Registrations
                    .Include(r => r.User)
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(r => r.Id == p.RegistrationId);
                if (registration?.User == null || registration.Event == null) continue;

                var certNumber = $"CERT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20);
                var pdfBytes = CertificateGenerator.GenerateCertificate(
                    registration.User.FullName ?? registration.User.Email,
                    registration.Event.Title,
                    registration.Event.StartDate,
                    certNumber
                );

                var certDir = Path.Combine(_webHostEnvironment.WebRootPath, "certificates");
                Directory.CreateDirectory(certDir);
                var fileName = $"cert-{certNumber}.pdf";
                var filePath = Path.Combine(certDir, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                var certificate = new Certificate
                {
                    EventId = registration.EventId,
                    UserId = registration.UserId,
                    CertificateNumber = certNumber,
                    IssuedAt = DateTime.Now,
                    FilePath = $"/certificates/{fileName}",
                    IsTemplateBased = false,
                    SentAt = DateTime.Now,
                    SentToEmail = registration.User.Email,
                    IsApproved = true
                };
                _context.Certificates.Add(certificate);
                approved++;
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{approved} certificate(s) approved successfully.";
            return RedirectToAction(nameof(Event), new { eventId });
        }
    }
}