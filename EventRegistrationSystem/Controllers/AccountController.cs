using AutoMapper;
using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.Repositories;
using EventRegistrationSystem.Services;
using EventRegistrationSystem.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventRegistrationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _service;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IAuditLogService _auditLogService;

        public AccountController(IAuthService service,
                               IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IEmailService emailService, ILogger<AuthRepository> logger, IAuditLogService auditLogService)
        {
            _service = service;
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new LoginDTO
                {
                    Email = vm.Email,
                    Password = vm.Password,
                    RememberMe = vm.RememberMe
                };

                bool success = await _service.LoginAsync(dto);

                if (!success)
                {
                    var user = await _userManager.FindByEmailAsync(vm.Email);
                    if (user != null && !user.IsVerified)
                    {
                        ModelState.AddModelError("", "Your email is not verified. Please verify your email first.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt. Please check your credentials.");
                    }
                    return View(vm);
                }

                var loggedInUser = await _userManager.FindByEmailAsync(vm.Email);
                if (loggedInUser != null)
                {
                    // Create claims
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loggedInUser.Id.ToString()),
                new Claim(ClaimTypes.Email, loggedInUser.Email),
                new Claim(ClaimTypes.Name, loggedInUser.FullName ?? loggedInUser.Email),
                new Claim("IsVerified", loggedInUser.IsVerified.ToString())
            };

                    // Add roles
                    var roles = await _userManager.GetRolesAsync(loggedInUser);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = vm.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(vm.RememberMe ? 30 : 1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["Success"] = $"Welcome back, {loggedInUser.FullName ?? "User"}!";
                    await _auditLogService.LogAsync(loggedInUser, "Login", "User logged in successfully");

                    // 👇 Role‑based redirect
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (roles.Contains("Staff"))
                        return RedirectToAction("Scan", "Staff");
                    else
                    {
                        return RedirectToAction("Index", "User");
                    }
                }
                else
                {
                    TempData["Error"] = "User not found after successful login.";
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", vm.Email);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _service.LogoutAsync();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(vm.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(vm);
                }

                var dto = new RegisterDTO
                {
                    Email = vm.Email,
                    FullName = vm.FullName,
                    Password = vm.Password,
                    PhoneNumber = vm.PhoneNumber,
                    Gender = vm.Gender,
                    BirthDate = vm.BirthDate,
                    Address = vm.Address
                };

                var success = await _service.RegisterAsync(dto);

                if (!success)
                {
                    // Registration failed for other reasons – you can add a generic error
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                    return View(vm);
                }

                TempData["Success"] = "Registration successful! Please check your email for OTP.";
                return RedirectToAction("VerifyOTP", new { email = vm.Email });
            }
            catch (Exception ex)
            {
                // Log exception
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyOTP(string email)
        {
            // Check if user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "User not found. Please register again.";
                return RedirectToAction("Register");
            }

            var viewModel = new VerifyOTPViewModel
            {
                Email = email,
                CanResend = CanResendOTP(user.OTPExpiry)
            };

            if (user.OTPExpiry.HasValue)
            {
                ViewBag.SecondsRemaining = (int)(user.OTPExpiry.Value - DateTime.Now).TotalSeconds;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var dto = new VerifyOTPDTO
                {
                    Email = vm.Email?.Trim() ?? string.Empty,
                    OTP = vm.OTP?.Trim() ?? string.Empty
                };

                Console.WriteLine($"Attempting OTP verification for: {dto.Email}");
                Console.WriteLine($"OTP provided: {dto.OTP}");

                var success = await _service.VerifyOTPAsync(dto);

                if (!success)
                {
                    TempData["Error"] = "Invalid OTP or OTP has expired. Please try again.";

                    // Get user to check if we can resend
                    var user = await _userManager.FindByEmailAsync(vm.Email);
                    vm.CanResend = CanResendOTP(user?.OTPExpiry);

                    if (user?.OTPExpiry.HasValue == true)
                    {
                        ViewBag.SecondsRemaining = (int)(user.OTPExpiry.Value - DateTime.Now).TotalSeconds;
                    }

                    return View(vm);
                }

                TempData["Success"] = "Email verified successfully! You can now login.";
                return RedirectToAction("Login", new { email = dto.Email });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOTP(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Register");
                }

                // Check if we can resend (OTP expired or doesn't exist)
                if (!CanResendOTP(user.OTPExpiry))
                {
                    var timeLeft = user.OTPExpiry.HasValue ?
                        (user.OTPExpiry.Value - DateTime.Now).TotalSeconds : 0;

                    TempData["Error"] = $"Please wait {Math.Ceiling(timeLeft)} seconds before requesting a new OTP.";
                    return RedirectToAction("VerifyOTP", new { email });
                }

                // Generate new OTP
                string newOtp = OTPGenerator.GenerateOTP();
                user.EmailOTP = newOtp;
                user.OTPExpiry = DateTime.Now.AddMinutes(10);

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Send email
                    await _emailService.SendAsync(
                        user.Email,
                        "New OTP Verification Code - Event System",
                        $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2>New OTP Request</h2>
                            <p>Your new OTP verification code is:</p>
                            <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; font-size: 24px; font-weight: bold; text-align: center;'>
                                {newOtp}
                            </div>
                            <p>This OTP is valid for 10 minutes.</p>
                            <hr>
                            <p>If you didn't request this, please ignore this email.</p>
                        </div>"
                    );

                    TempData["Success"] = "New OTP has been sent to your email.";
                }
                else
                {
                    TempData["Error"] = "Failed to generate new OTP. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to resend OTP: {ex.Message}";
            }

            return RedirectToAction("VerifyOTP", new { email });
        }

        private bool CanResendOTP(DateTime? otpExpiry)
        {
            // Can resend if OTP is expired or doesn't exist
            return !otpExpiry.HasValue || otpExpiry.Value <= DateTime.Now;
        }
        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "This email is not registered.");
                return View(model);
            }

            // Generate OTP and store it
            var otp = OTPGenerator.GenerateOTP();
            user.EmailOTP = otp;
            user.OTPExpiry = DateTime.Now.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            // Send OTP via email
            await _emailService.SendAsync(user.Email, "Password Reset OTP",
                $@"
        <h2>Password Reset Request</h2>
        <p>Your OTP to reset your password is:</p>
        <h1 style='color: #6366f1;'>{otp}</h1>
        <p>This code is valid for 10 minutes.</p>
        <p>If you did not request this, please ignore this email.</p>");

            TempData["Success"] = "An OTP has been sent to your email.";
            return RedirectToAction(nameof(VerifyResetOtp), new { email = user.Email });
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/VerifyResetOtp
        [HttpGet]
        public IActionResult VerifyResetOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            var vm = new VerifyOTPViewModel { Email = email };
            return View(vm);
        }

        // POST: /Account/VerifyResetOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetOtp(VerifyOTPViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null || user.EmailOTP != vm.OTP || user.OTPExpiry < DateTime.Now)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View(vm);
            }

            // Clear OTP
            user.EmailOTP = null;
            user.OTPExpiry = null;
            await _userManager.UpdateAsync(user);

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = user.Email;

            return RedirectToAction(nameof(ResetPassword));
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["ResetToken"] == null || TempData["ResetEmail"] == null)
                return RedirectToAction(nameof(ForgotPassword));

            var vm = new ResetPasswordViewModel
            {
                Email = TempData["ResetEmail"].ToString(),
                Token = TempData["ResetToken"].ToString()
            };
            return View(vm);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetPasswordConfirmation)); // don't reveal

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // Optional: Resend OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendResetOtp(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var otp = OTPGenerator.GenerateOTP();
            user.EmailOTP = otp;
            user.OTPExpiry = DateTime.Now.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            await _emailService.SendAsync(user.Email, "Password Reset OTP",
                $"Your new OTP is: <b>{otp}</b>. Valid for 10 minutes.");

            TempData["Success"] = "A new OTP has been sent to your email.";
            return RedirectToAction(nameof(VerifyResetOtp), new { email = user.Email });
        }
    }
}