using AutoMapper;
using EventRegistrationSystem.Services;
using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Repositories;

namespace EventSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly EmailService _emailService;
        private readonly IMapper _mapper;

        public AuthService(IAuthRepository repo,
                   IMapper mapper,
                   EmailService emailService)
        {
            _repo = repo;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<bool> LoginAsync(LoginDTO dto)
        {
            return await _repo.LoginAsync(dto.Email, dto.Password, dto.RememberMe);
        }

        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            return await _repo.RegisterAsync(dto);
        }

        public async Task<bool> VerifyOTPAsync(VerifyOTPDTO dto)
        {
            return await _repo.VerifyOTPAsync(dto);
        }
        public async Task<bool> ResendOTPAsync(string email) // Add this
        {
            return await _repo.ResendOTPAsync(email);
        }

        public async Task LogoutAsync()
        {
            await _repo.LogoutAsync();
        }
    }
}
