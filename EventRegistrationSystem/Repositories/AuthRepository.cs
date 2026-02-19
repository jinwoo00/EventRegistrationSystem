using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;
using EventRegistrationSystem.Services;
using Microsoft.Extensions.Logging;

namespace EventRegistrationSystem.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, EmailService emailService, ILogger<AuthRepository> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found.", email);
                return false;
            }

            // BLOCK LOGIN IF EMAIL NOT VERIFIED
            if (!user.IsVerified)
            {
                _logger.LogWarning("Login failed: User {Email} is not verified.", email);
                return false;
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, password, rememberMe, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Password sign-in failed for user {Email}.", email);
            }

            return result.Succeeded;
        }


        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogError("Registration failed: Email is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogError("Registration failed: Password is required");
                return false;
            }

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User {Email} already exists", dto.Email);
                    return false;
                }

                // Generate OTP
                var otp = OTPGenerator.GenerateOTP();
                _logger.LogInformation("Generated OTP for {Email}: {OTP}", dto.Email, otp);

                // Log OTP to console and file for debugging
                Console.WriteLine($"OTP for {dto.Email}: {otp}");
                File.AppendAllText("otp_debug.txt", $"{DateTime.Now}: {dto.Email} - {otp}\n");

                // Create user
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty,
                    Gender = dto.Gender ?? "Other",
                    BirthDate = dto.BirthDate,
                    Address = dto.Address ?? string.Empty,
                    EmailOTP = otp,
                    OTPExpiry = DateTime.Now.AddMinutes(10),
                    IsVerified = false,
                    EmailConfirmed = false
                };

                // Create user
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed for {Email}", dto.Email);
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("Error: {Code} - {Description}", error.Code, error.Description);
                    }
                    return false;
                }

                _logger.LogInformation("User created successfully. ID: {UserId}", user.Id);

                // Send OTP email
                try
                {
                    // Simple HTML email
                    string emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Event System - Email Verification</h2>
                    <p>Hello {user.FullName},</p>
                    <p>Your OTP verification code is:</p>
                    <h1 style='color: #007bff;'>{otp}</h1>
                    <p>This code is valid for 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <hr>
                    <p>Thank you,<br>Event System Team</p>
                </body>
                </html>";

                    await _emailService.SendAsync(
                        user.Email,
                        "Event System - OTP Verification Code",
                        emailBody
                    );

                    _logger.LogInformation("OTP email sent to {Email}", dto.Email);
                    Console.WriteLine($"✓ Email sent to {dto.Email}");

                    return true;
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send OTP email to {Email}", dto.Email);

                    // Delete user if email fails
                    try
                    {
                        await _userManager.DeleteAsync(user);
                        _logger.LogInformation("Deleted user {Email} after email failure", dto.Email);
                    }
                    catch { }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
                return false;
            }
        }
        public async Task<bool> VerifyOTPAsync(VerifyOTPDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;

            if (user.EmailOTP != dto.OTP || user.OTPExpiry < DateTime.Now)
                return false;

            user.EmailConfirmed = true;
            user.IsVerified = true;
            user.EmailOTP = null;
            user.OTPExpiry = null;

            await _userManager.UpdateAsync(user);
            return true;
        }
        public async Task<bool> ResendOTPAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null) return false;

                // Generate new OTP
                string newOtp = OTPGenerator.GenerateOTP();
                user.EmailOTP = newOtp;
                user.OTPExpiry = DateTime.Now.AddMinutes(10);

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded) return false;

                // Send email
                await _emailService.SendAsync(
                    user.Email,
                    "New OTP Verification Code - Event System",
                    $"Your new OTP is: <b>{newOtp}</b>. Valid for 10 minutes."
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP for {Email}", email);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

    }
}
