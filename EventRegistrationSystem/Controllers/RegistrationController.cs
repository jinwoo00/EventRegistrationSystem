using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Controllers
{
    [Authorize] // only logged-in users can access their own tickets
    public class RegistrationController : Controller
    {
        private readonly IRegistrationRepository _regRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationController(IRegistrationRepository regRepo, UserManager<ApplicationUser> userManager)
        {
            _regRepo = regRepo;
            _userManager = userManager;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> TicketQr(int registrationId)
        {
            var registration = await _regRepo.GetByIdAsync(registrationId);
            if (registration == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (registration.UserId != userId) return Forbid();

            var url = Url.Action("ProcessScan", "Staff", new { id = registrationId }, Request.Scheme);
            // Log the URL to Visual Studio Output window (Debug → Windows → Output)
            System.Diagnostics.Debug.WriteLine($"QR URL: {url}");

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrBytes = qrCode.GetGraphic(50);
                return File(qrBytes, "image/png");
            }
        }
    }
}