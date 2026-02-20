using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly ILogger<AuditLogsController> _logger;
        private readonly ApplicationDbContext _context;

        public AuditLogsController(IAuditLogRepository auditLogRepo, ILogger<AuditLogsController> logger, ApplicationDbContext context)
        {
            _auditLogRepo = auditLogRepo;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? actionFilter, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            search = string.IsNullOrEmpty(search) ? null : search;
            actionFilter = string.IsNullOrEmpty(actionFilter) ? null : actionFilter;

            int pageSize = 15;
            var pagedResult = await _auditLogRepo.GetPagedAsync(
                search: search,
                action: actionFilter,  // ✅ pass it correctly
                fromDate: fromDate,
                toDate: toDate,
                page: page,
                pageSize: pageSize
            );

            var actions = await _auditLogRepo.GetDistinctActionsAsync();
            ViewBag.Actions = actions.Select(a => new SelectListItem
            {
                Value = a,
                Text = a,
                Selected = a == actionFilter  // ✅ renamed
            }).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentAction = actionFilter;  // ✅ renamed
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

            return View("~/Views/Admin/AuditLogs/Index.cshtml", pagedResult);
        }

    }
}