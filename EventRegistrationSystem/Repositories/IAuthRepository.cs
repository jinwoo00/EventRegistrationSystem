using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> LoginAsync(string email, string password, bool rememberMe);
        Task<bool> RegisterAsync(RegisterDTO dto);
        Task<bool> VerifyOTPAsync(VerifyOTPDTO dto);
        Task<bool> ResendOTPAsync(string email);
        Task LogoutAsync();
    }
}