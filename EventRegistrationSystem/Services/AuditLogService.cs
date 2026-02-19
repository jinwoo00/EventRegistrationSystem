using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrationSystem.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string? userId = null, string? userEmail = null, string? userRole = null, string? description = null, string? ipAddress = null)
        {
            var ip = ipAddress ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                Action = action,
                UserId = userId,
                UserEmail = userEmail,
                UserRole = userRole,
                Description = description,
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogAsync(ApplicationUser user, string action, string? description = null, string? ipAddress = null)
        {
            var role = (await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .FirstOrDefaultAsync()) ?? "User";

            await LogAsync(action, user.Id, user.Email, role, description, ipAddress);
        }
    }
}