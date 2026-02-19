using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Repositories
{
    public interface IRegistrationRepository
    {
        Task<List<Registration>> GetAllAsync(int? eventId = null, string? search = null, bool? checkedIn = null);
        Task<Registration?> GetByIdAsync(int id);
        Task<bool> ToggleCheckInAsync(int id, string userId);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountByEventAsync(int eventId);
        Task<int> GetTotalRegistrationsAsync();
        Task<int> GetTodayRegistrationsAsync();

        Task<int> GetFilteredCheckedInCountAsync(int? eventId = null, string? search = null);
        Task<int> GetFilteredTodayCountAsync(int? eventId = null, string? search = null, bool? checkedIn = null);
        Task<bool> RegisterUserForEventAsync(int eventId, string userId);
        Task<bool> IsUserRegisteredAsync(int eventId, string userId);

        Task<PagedResult<RegistrationListItemDTO>> GetPagedDtoAsync(
            int page = 1,
            int pageSize = 10,
            int? eventId = null,
            string? search = null,
            bool? checkedIn = null);
        Task<PagedResult<AttendanceListItemDto>> GetAttendanceListAsync(
            int? eventId = null,
            string? search = null,
            bool? checkedIn = null,
            int page = 1,
            int pageSize = 10);
        Task<bool> CheckInAsync(int registrationId, string userId);
        Task<bool> CheckOutAsync(int registrationId, string userId);
        Task<int> GetPendingCountAsync(int? eventId = null);
        Task<int> GetCheckedInCountAsync(int? eventId = null);
        Task<int> GetCheckedOutCountAsync(int? eventId = null);

        Task<List<UserRegistrationDTO>> GetUserRegistrationsAsync(string userId);
    }
}