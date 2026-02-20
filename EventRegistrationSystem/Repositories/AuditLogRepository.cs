using EventRegistrationSystem.Data;
using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AuditLogListItemDTO>> GetPagedAsync(string? search = null, string? action = null,DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 10)
        {
            // ✅ Start as plain IQueryable — NO ordering yet
            IQueryable<AuditLog> query = _context.AuditLogs;

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l =>
                    (l.UserEmail != null && l.UserEmail.Contains(search)) ||
                    (l.Description != null && l.Description.Contains(search)) ||
                    (l.IpAddress != null && l.IpAddress.Contains(search)));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(l => l.Action == action);

            if (fromDate.HasValue)
                query = query.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.Timestamp <= toDate.Value);

            // ✅ Count BEFORE ordering
            var totalCount = await query.CountAsync();

            // ✅ Order THEN page
            var items = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogListItemDTO
                {
                    Id = l.Id,
                    Action = l.Action,
                    UserEmail = l.UserEmail,
                    UserRole = l.UserRole,
                    Description = l.Description,
                    IpAddress = l.IpAddress,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            return new PagedResult<AuditLogListItemDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public async Task<List<string>> GetDistinctActionsAsync()
        {
            return await _context.AuditLogs
                .Select(l => l.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }
    }
}