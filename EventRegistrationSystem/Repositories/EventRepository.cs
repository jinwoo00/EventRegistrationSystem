using Microsoft.EntityFrameworkCore;
using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== EXISTING METHODS ==========
        public async Task<List<Event>> GetAllAsync(bool includeUnpublished = false)
        {
            var query = _context.Events.AsQueryable();
            if (!includeUnpublished)
                query = query.Where(e => e.IsPublished);
            return await query.OrderByDescending(e => e.StartDate).ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
            => await _context.Events.FindAsync(id);

        public async Task<Event> CreateAsync(Event ev)
        {
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return ev;
        }

        public async Task<Event> UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
            return ev;
        }

        public async Task DeleteAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Event>> GetPublishedEventsAsync()
            => await _context.Events
                .Where(e => e.IsPublished && e.EndDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

        public async Task<int> GetRegistrationCountAsync(int eventId)
            => await _context.Registrations.CountAsync(r => r.EventId == eventId);

        public async Task<int> GetCheckedInCountForEventAsync(int eventId)
            => await _context.Registrations
                .Where(r => r.EventId == eventId && r.IsCheckedIn)
                .CountAsync();

        // ========== NEW METHODS FOR STATUS FILTERING ==========
        public async Task<List<Event>> GetIncomingEventsAsync(string? search = null)
        {
            var query = _context.Events
                .Where(e => e.EndDate >= DateTime.Today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(search) ||
                                         (e.Description != null && e.Description.ToLower().Contains(search)) ||
                                         (e.Location != null && e.Location.ToLower().Contains(search)));
            }

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<List<Event>> GetPassedEventsAsync(string? search = null)
        {
            var query = _context.Events
                .Where(e => e.EndDate < DateTime.Today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(search) ||
                                         (e.Description != null && e.Description.ToLower().Contains(search)) ||
                                         (e.Location != null && e.Location.ToLower().Contains(search)));
            }

            return await query.OrderByDescending(e => e.EndDate).ToListAsync();
        }

        // ========== PAGED EVENTS WITH STATUS FILTER ==========
        public async Task<PagedResult<EventListItemDTO>> GetPagedEventsAsync(
            string? status = "incoming",
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Events.AsQueryable();

            // Apply status filter
            if (status == "incoming")
                query = query.Where(e => e.EndDate >= DateTime.Today);
            else if (status == "passed")
                query = query.Where(e => e.EndDate < DateTime.Today);
            // else "all" – no date filter

            // Apply search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(search) ||
                                         (e.Description != null && e.Description.ToLower().Contains(search)) ||
                                         (e.Location != null && e.Location.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            // Apply sorting and pagination
            IQueryable<Event> sortedQuery;
            if (status == "incoming")
                sortedQuery = query.OrderBy(e => e.StartDate);
            else if (status == "passed")
                sortedQuery = query.OrderByDescending(e => e.EndDate);
            else
                sortedQuery = query.OrderByDescending(e => e.StartDate); // fallback

            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventListItemDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    ImageUrl = e.ImageUrl,
                    StartDate = e.StartDate,
                    Location = e.Location,
                    IsPublished = e.IsPublished,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<EventListItemDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public async Task<int> GetPublishedCountAsync(string? search = null)
        {
            var query = _context.Events.Where(e => e.IsPublished);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
            return await query.CountAsync();
        }

        public async Task<int> GetDraftCountAsync(string? search = null)
        {
            var query = _context.Events.Where(e => !e.IsPublished);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
            return await query.CountAsync();
        }

        public async Task<int> GetThisMonthCountAsync()
        {
            var now = DateTime.Now;
            return await _context.Events
                .CountAsync(e => e.StartDate.Month == now.Month && e.StartDate.Year == now.Year);
        }
        public async Task<List<EventRegistrationSummaryDTO>> GetEventSummariesAsync()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EventRegistrationSummaryDTO
                {
                    EventId = e.Id,
                    EventTitle = e.Title,
                    EventStartDate = e.StartDate,
                    EventLocation = e.Location,
                    ImageUrl = e.ImageUrl,
                    TotalRegistrations = e.Registrations.Count,
                    CheckedInCount = e.Registrations.Count(r => r.IsCheckedIn),
                    CheckInPercentage = e.Registrations.Count > 0
                        ? Math.Round((double)e.Registrations.Count(r => r.IsCheckedIn) / e.Registrations.Count * 100, 1)
                        : 0
                })
                .ToListAsync();

            return events;
        }
        public async Task<PagedResult<EventRegistrationSummaryDTO>> GetPagedEventSummariesAsync(int page = 1, int pageSize = 6)
        {
            var query = _context.Events
                .OrderByDescending(e => e.StartDate)
                .Select(e => new EventRegistrationSummaryDTO
                {
                    EventId = e.Id,
                    EventTitle = e.Title,
                    EventStartDate = e.StartDate,
                    EventLocation = e.Location,
                    ImageUrl = e.ImageUrl,
                    TotalRegistrations = e.Registrations.Count,
                    CheckedInCount = e.Registrations.Count(r => r.IsCheckedIn),
                    CheckInPercentage = e.Registrations.Count > 0
                        ? Math.Round((double)e.Registrations.Count(r => r.IsCheckedIn) / e.Registrations.Count * 100, 1)
                        : 0,
                    CertificatesIssued = _context.Certificates.Count(c => c.EventId == e.Id) // 👈 direct count
                });

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<EventRegistrationSummaryDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}