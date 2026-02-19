using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.ViewModel.Admin;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class EventsController : Controller
    {
        private readonly IEventRepository _eventRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(IEventRepository eventRepo, IWebHostEnvironment webHostEnvironment)
        {
            _eventRepo = eventRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Events/Index
            public async Task<IActionResult> Index(
            string? status = "incoming",
            string? search = null,
            int page = 1)
        {
            int pageSize = 10;

            var pagedResult = await _eventRepo.GetPagedEventsAsync(
                status: status,
                search: search,
                page: page,
                pageSize: pageSize
            );

            // Stats counts (global or filtered? adjust as needed)
            ViewBag.TotalEvents = pagedResult.TotalCount; // total in current filter
            ViewBag.PublishedCount = await _eventRepo.GetPublishedCountAsync(search);
            ViewBag.DraftCount = await _eventRepo.GetDraftCountAsync(search);
            ViewBag.ThisMonthCount = await _eventRepo.GetThisMonthCountAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;

            return View("~/Views/Admin/Events/Index.cshtml", pagedResult);
        }

        // GET: Admin/Events/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/Events/EditCreate.cshtml", new EventViewModel());
        }

        // POST: Admin/Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var eventEntity = new Event
            {
                Title = vm.Title,
                Description = vm.Description,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Location = vm.Location,
                Capacity = vm.Capacity,
                IsPublished = vm.IsPublished,
                CreatedAt = DateTime.Now
            };

            // Handle poster upload
            if (vm.PosterFile != null && vm.PosterFile.Length > 0)
            {
                eventEntity.ImageUrl = await SavePosterAsync(vm.PosterFile);
            }

            await _eventRepo.CreateAsync(eventEntity);
            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null) return NotFound();

            var vm = new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                Location = ev.Location,
                Capacity = ev.Capacity,
                IsPublished = ev.IsPublished,
                ExistingImageUrl = ev.ImageUrl
            };
            return View("~/Views/Admin/Events/EditCreate.cshtml", vm);
        }

        // POST: Admin/Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null) return NotFound();

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.StartDate = vm.StartDate;
            ev.EndDate = vm.EndDate;
            ev.Location = vm.Location;
            ev.Capacity = vm.Capacity;
            ev.IsPublished = vm.IsPublished;

            // Update poster if new file uploaded
            if (vm.PosterFile != null && vm.PosterFile.Length > 0)
            {
                // Delete old poster if exists
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                    DeletePoster(ev.ImageUrl);

                ev.ImageUrl = await SavePosterAsync(vm.PosterFile);
            }

            await _eventRepo.UpdateAsync(ev);
            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Events/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev != null && !string.IsNullOrEmpty(ev.ImageUrl))
                DeletePoster(ev.ImageUrl);

            await _eventRepo.DeleteAsync(id);
            TempData["Success"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper: Save poster file
        private async Task<string> SavePosterAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/events/" + uniqueFileName;
        }

        // Helper: Delete poster file
        private void DeletePoster(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Events()
        {
            var events = await _eventRepo.GetPublishedEventsAsync();
            return View(events);
        }
        // GET: Admin/Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null) return NotFound();

            var totalRegistrations = await _eventRepo.GetRegistrationCountAsync(id);
            var checkedInCount = await _eventRepo.GetCheckedInCountForEventAsync(id);

            // 🔍 Debug output – check Visual Studio Output window
            System.Diagnostics.Debug.WriteLine($"Event ID {id}: Total = {totalRegistrations}, CheckedIn = {checkedInCount}");

            var viewModel = new EventDetailsViewModel
            {
                Event = ev,
                TotalRegistrations = totalRegistrations,
                CheckedInCount = checkedInCount,
                CheckInPercentage = totalRegistrations > 0
                    ? Math.Round((double)checkedInCount / totalRegistrations * 100, 1)
                    : 0
            };

            return View("~/Views/Admin/Events/Details.cshtml", viewModel);
        }
    }

}