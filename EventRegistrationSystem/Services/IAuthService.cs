using EventRegistrationSystem.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrationSystem.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginDTO dto);
        Task<bool> RegisterAsync(RegisterDTO dto);
        Task<bool> VerifyOTPAsync(VerifyOTPDTO dto);
        Task LogoutAsync();
    }
}
