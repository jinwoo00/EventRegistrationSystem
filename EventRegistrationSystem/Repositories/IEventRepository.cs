using EventRegistrationSystem.Models;
using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Repositories
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllAsync(bool includeUnpublished = false);
        Task<Event?> GetByIdAsync(int id);
        Task<Event> CreateAsync(Event ev);
        Task<Event> UpdateAsync(Event ev);
        Task DeleteAsync(int id);
        Task<List<Event>> GetPublishedEventsAsync(); // for participant feed
        Task<int> GetRegistrationCountAsync(int eventId);
        Task<int> GetCheckedInCountForEventAsync(int eventId);
        Task<List<Event>> GetIncomingEventsAsync(string? search = null);
        Task<List<Event>> GetPassedEventsAsync(string? search = null);
        Task<int> GetPublishedCountAsync(string? search = null);
        Task<int> GetDraftCountAsync(string? search = null);
        Task<int> GetThisMonthCountAsync();
        Task<List<EventRegistrationSummaryDTO>> GetEventSummariesAsync();
        Task<PagedResult<EventListItemDTO>> GetPagedEventsAsync(
            string? status = "incoming",   // "incoming", "passed", or "all"
            string? search = null,
            int page = 1,
            int pageSize = 10);
        Task<PagedResult<EventRegistrationSummaryDTO>> GetPagedEventSummariesAsync(int page = 1, int pageSize = 6);

    }
}