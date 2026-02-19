using EventRegistrationSystem.DTOs;
using static EventRegistrationSystem.DTOs.AdminDashboardDTO;
namespace EventRegistrationSystem.Repositories

{
    public interface IAdminDashboardRepository
    {
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<List<RegistrationTrendDto>> GetRegistrationTrendAsync(int days = 30);
        Task<List<EventAttendanceRateDto>> GetEventAttendanceRatesAsync(int top = 10);
        Task<AttendanceStatusDto> GetOverallAttendanceStatusAsync();
    }
}