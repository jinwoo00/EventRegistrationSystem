using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Repositories
{
    public interface IAuditLogRepository
    {
        Task<PagedResult<AuditLogListItemDTO>> GetPagedAsync(
            string? search = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10);

        Task<List<string>> GetDistinctActionsAsync();
    }
}