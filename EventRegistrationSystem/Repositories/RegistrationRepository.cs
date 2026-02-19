using EventRegistrationSystem.Data;
using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Optional: inject ILogger for production logging

namespace EventRegistrationSystem.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegistrationRepository>? _logger; // Optional

        public RegistrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Optional: constructor with logger
        // public RegistrationRepository(ApplicationDbContext context, ILogger<RegistrationRepository> logger)
        // {
        //     _context = context;
        //     _logger = logger;
        // }

        public async Task<List<Registration>> GetAllAsync(int? eventId = null, string? search = null, bool? checkedIn = null)
        {
            var query = _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }

            if (checkedIn.HasValue)
                query = query.Where(r => r.IsCheckedIn == checkedIn.Value);

            return await query
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();
        }

        public async Task<Registration?> GetByIdAsync(int id)
        {
            return await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ToggleCheckInAsync(int id, string userId)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null) return false;

            registration.IsCheckedIn = !registration.IsCheckedIn;
            registration.CheckedInAt = registration.IsCheckedIn ? DateTime.Now : null;

            if (registration.IsCheckedIn)
            {
                var attendance = new Attendance
                {
                    RegistrationId = registration.Id,
                    CheckedInAt = DateTime.Now,
                    CheckedInBy = userId
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.RegistrationId == registration.Id);
                if (attendance != null)
                    _context.Attendances.Remove(attendance);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null) return false;

            var attendances = await _context.Attendances
                .Where(a => a.RegistrationId == id).ToListAsync();
            _context.Attendances.RemoveRange(attendances);

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountByEventAsync(int eventId)
        {
            return await _context.Registrations
                .Where(r => r.EventId == eventId)
                .CountAsync();
        }

        public async Task<int> GetTotalRegistrationsAsync()
        {
            return await _context.Registrations.CountAsync();
        }

        public async Task<int> GetTodayRegistrationsAsync()
        {
            var today = DateTime.Today;
            return await _context.Registrations
                .Where(r => r.RegisteredAt.Date == today)
                .CountAsync();
        }

        public async Task<PagedResult<Registration>> GetPagedAsync(
            int page = 1,
            int pageSize = 10,
            int? eventId = null,
            string? search = null,
            bool? checkedIn = null)
        {
            var query = _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }
            if (checkedIn.HasValue)
                query = query.Where(r => r.IsCheckedIn == checkedIn.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Registration>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetFilteredCheckedInCountAsync(int? eventId = null, string? search = null)
        {
            var query = _context.Registrations.AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }

            return await query.CountAsync(r => r.IsCheckedIn);
        }

        public async Task<int> GetFilteredTodayCountAsync(int? eventId = null, string? search = null, bool? checkedIn = null)
        {
            var today = DateTime.Today;
            var query = _context.Registrations
                .Where(r => r.RegisteredAt.Date == today);

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }

            if (checkedIn.HasValue)
                query = query.Where(r => r.IsCheckedIn == checkedIn.Value);

            return await query.CountAsync();
        }

        public async Task<PagedResult<RegistrationListItemDTO>> GetPagedDtoAsync(
            int page = 1,
            int pageSize = 10,
            int? eventId = null,
            string? search = null,
            bool? checkedIn = null)
        {
            var query = _context.Registrations
                .AsNoTracking()
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }

            if (checkedIn.HasValue)
                query = query.Where(r => r.IsCheckedIn == checkedIn.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RegistrationListItemDTO
                {
                    Id = r.Id,
                    AttendeeName = r.User != null ? r.User.FullName : null,
                    AttendeeEmail = r.User != null ? r.User.Email : null,
                    EventTitle = r.Event != null ? r.Event.Title : null,
                    RegisteredAt = r.RegisteredAt,
                    TicketType = r.TicketType,
                    IsCheckedIn = r.IsCheckedIn,
                    CheckedInAt = r.CheckedInAt
                })
                .ToListAsync();

            return new PagedResult<RegistrationListItemDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> RegisterUserForEventAsync(int eventId, string userId)
        {
            if (await IsUserRegisteredAsync(eventId, userId))
                return false;

            var registration = new Registration
            {
                EventId = eventId,
                UserId = userId,
                RegisteredAt = DateTime.Now,
                IsCheckedIn = false,
                TicketType = "General"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserRegisteredAsync(int eventId, string userId)
        {
            return await _context.Registrations
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        }

        public async Task<PagedResult<AttendanceListItemDto>> GetAttendanceListAsync(
            int? eventId = null,
            string? search = null,
            bool? checkedIn = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Registrations
                .Include(r => r.User)
                .Include(r => r.Event)
                .AsNoTracking()
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.User != null && (r.User.FullName != null && r.User.FullName.ToLower().Contains(search) ||
                                        r.User.Email != null && r.User.Email.ToLower().Contains(search))) ||
                    (r.Event != null && r.Event.Title.ToLower().Contains(search)) ||
                    (r.TicketType != null && r.TicketType.ToLower().Contains(search)));
            }

            if (checkedIn.HasValue)
                query = query.Where(r => r.IsCheckedIn == checkedIn.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.IsCheckedIn)
                .ThenByDescending(r => r.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AttendanceListItemDto
                {
                    RegistrationId = r.Id,
                    AttendeeName = r.User != null ? r.User.FullName : null,
                    AttendeeEmail = r.User != null ? r.User.Email : null,
                    EventTitle = r.Event != null ? r.Event.Title : null,
                    RegisteredAt = r.RegisteredAt,
                    IsCheckedIn = r.IsCheckedIn,
                    CheckedInAt = r.CheckedInAt,
                    CheckedOutAt = r.CheckedOutAt,
                    TicketType = r.TicketType
                })
                .ToListAsync();

            return new PagedResult<AttendanceListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> CheckInAsync(int registrationId, string userId)
        {
            try
            {
                var reg = await _context.Registrations.FindAsync(registrationId);
                if (reg == null)
                {
                    Console.WriteLine($"CheckInAsync: Registration {registrationId} not found.");
                    return false;
                }

                if (reg.IsCheckedIn)
                {
                    Console.WriteLine($"CheckInAsync: Registration {registrationId} already checked in.");
                    return false;
                }

                reg.IsCheckedIn = true;
                reg.CheckedInAt = DateTime.Now;
                reg.CheckedOutAt = null;

                // Mark as modified to ensure EF tracks it (optional but safe)
                _context.Entry(reg).State = EntityState.Modified;

                var saved = await _context.SaveChangesAsync();
                Console.WriteLine($"CheckInAsync: Saved {saved} changes for registration {registrationId}.");
                return saved > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckInAsync error: {ex}");
                return false;
            }
        }

        public async Task<bool> CheckOutAsync(int registrationId, string userId)
        {
            try
            {
                var reg = await _context.Registrations.FindAsync(registrationId);
                if (reg == null || !reg.IsCheckedIn)
                {
                    _logger?.LogWarning("CheckOutAsync: Registration {RegistrationId} not found or not checked in.", registrationId);
                    return false;
                }

                reg.CheckedOutAt = DateTime.Now;

                // Update attendance record if exists
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.RegistrationId == registrationId && a.CheckedOutAt == null);
                if (attendance != null)
                {
                    attendance.CheckedOutAt = DateTime.Now;
                    attendance.CheckedOutBy = userId;
                }

                await _context.SaveChangesAsync();
                _logger?.LogInformation("CheckOutAsync: Checked out registration {RegistrationId}.", registrationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CheckOutAsync error for registration {RegistrationId}", registrationId);
                return false;
            }
        }

        public async Task<int> GetPendingCountAsync(int? eventId = null)
        {
            var query = _context.Registrations.Where(r => !r.IsCheckedIn);
            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);
            return await query.CountAsync();
        }

        public async Task<int> GetCheckedInCountAsync(int? eventId = null)
        {
            var query = _context.Registrations.Where(r => r.IsCheckedIn && r.CheckedOutAt == null);
            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);
            return await query.CountAsync();
        }

        public async Task<int> GetCheckedOutCountAsync(int? eventId = null)
        {
            var query = _context.Registrations.Where(r => r.CheckedOutAt != null);
            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);
            return await query.CountAsync();
        }

        public async Task<List<UserRegistrationDTO>> GetUserRegistrationsAsync(string userId)
        {
            return await _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .OrderByDescending(r => r.RegisteredAt)
                .Select(r => new UserRegistrationDTO
                {
                    RegistrationId = r.Id,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    EventStartDate = r.Event.StartDate,
                    EventLocation = r.Event.Location,
                    EventImageUrl = r.Event.ImageUrl,
                    RegisteredAt = r.RegisteredAt,
                    IsCheckedIn = r.IsCheckedIn,
                    CheckedInAt = r.CheckedInAt,
                    CheckedOutAt = r.CheckedOutAt
                })
                .ToListAsync();
        }
    }
}