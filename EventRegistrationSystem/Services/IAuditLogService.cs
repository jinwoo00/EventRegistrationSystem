using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string? userId = null, string? userEmail = null, string? userRole = null, string? description = null, string? ipAddress = null);
        Task LogAsync(ApplicationUser user, string action, string? description = null, string? ipAddress = null);
    }
}