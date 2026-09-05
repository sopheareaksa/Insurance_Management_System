using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Insurance_Management_System.Models;
using Insurance_Management_System.ViewModels;
using Insurance_Management_System.DTOs;
using Insurance_Management_System.Services;

namespace Insurance_Management_System.Controllers;

public class AuthController : Controller
{
    private readonly InsuranceContext _context;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;

    public AuthController(InsuranceContext context, IMemoryCache cache, IEmailService emailService)
    {
        _context = context;
        _cache = cache;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult RegisterCustomer(string? username = null, string? email = null)
    {
        var model = new RegisterCustomerViewModel
        {
            Username = username ?? string.Empty,
            Email = email ?? string.Empty
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var parameters = new[]
            {
                new SqlParameter("@Username", model.Username),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@Phone", model.Phone),
                new SqlParameter("@Address", (object?)model.Address ?? DBNull.Value),
                new SqlParameter("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@NationalId", (object?)model.NationalId ?? DBNull.Value)
            };
            var result = await _context.Database
                .SqlQueryRaw<CustomerRegisterResult>(
                    "EXEC dbo.sp_RegisterCustomer @Username, @Email, @PasswordHash, @FullName, @Phone, @Address, @DateOfBirth, @NationalId",
                    parameters)
                .ToListAsync();

            var registeredCustomer = result.FirstOrDefault();

            TempData["SuccessMessage"] = registeredCustomer?.Message ?? "Registration successful! Please login.";
            return RedirectToAction(nameof(Login));
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601)
            {
                ModelState.AddModelError(string.Empty, "Username, Email, or National ID already exists.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
            }

            return View(model);
        }
    }

    [HttpGet]
    public IActionResult RegisterSeller(string? username = null, string? email = null)
    {
        var model = new RegisterSellerViewModel
        {
            Username = username ?? string.Empty,
            Email = email ?? string.Empty
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterSeller(RegisterSellerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var parameters = new[]
            {
                new SqlParameter("@Username", model.Username),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@CompanyName", model.CompanyName),
                new SqlParameter("@CompanyEmail", model.CompanyEmail),
                new SqlParameter("@CompanyPhone", model.CompanyPhone),
                new SqlParameter("@CompanyAddress", (object?)model.CompanyAddress ?? DBNull.Value),
                new SqlParameter("@CommissionRate", 10.00m),
                new SqlParameter("@ApprovalStatus", "Pending"),
                new SqlParameter("@LeadAgentFullName", model.LeadAgentFullName),
                new SqlParameter("@Branch", string.IsNullOrWhiteSpace(model.Branch) ? "Main Branch" : model.Branch)
            };
            var result = await _context.Database
                .SqlQueryRaw<SellerRegisterResult>(
                    "EXEC dbo.sp_RegisterSellerCompany @Username, @Email, @PasswordHash, @CompanyName, @CompanyEmail, @CompanyPhone, @CompanyAddress, @CommissionRate, @ApprovalStatus, @LeadAgentFullName, @Branch",
                    parameters)
                .ToListAsync();

            var registeredSeller = result.FirstOrDefault();

            TempData["SuccessMessage"] = $"Company registered successfully under status '{registeredSeller?.ApprovalStatus}'. Assigned Code: {registeredSeller?.RegNo}";
            return RedirectToAction(nameof(Login));
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601)
            {
                ModelState.AddModelError(string.Empty, "Username, Email, or Company details already exist.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
            }

            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["ErrorMessage"] = "Please provide both Email/Username and Password.";
            return RedirectToAction(nameof(Login));
        }

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.email == email || u.username == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.passwordHash))
        {
            TempData["ErrorMessage"] = "Invalid credentials. Please verify your email/username and password.";
            return RedirectToAction(nameof(Login));
        }

        if (user.status != "Active")
        {
            TempData["ErrorMessage"] = $"Account status is '{user.status}'. Please contact administrator.";
            return RedirectToAction(nameof(Login));
        }

        user.lastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Welcome back, {user.username}! Signed in as {user.role}.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOTP(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("ForgotPassword", model);
        }

        var normalizedEmail = model.Email.Trim().ToLower();
        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.email.ToLower() == normalizedEmail);
        if (user == null)
        {
            ModelState.AddModelError(nameof(model.Email), "No account registered with this email address.");
            return View("ForgotPassword", model);
        }
        string otp = Random.Shared.Next(100000, 999999).ToString();
        string cacheKey = $"OTP_{normalizedEmail}";
        _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

        try
        {
            await _emailService.SendOtpEmailAsync(model.Email.Trim(), otp);
            TempData["SuccessMessage"] = $"A 6-digit OTP code has been sent to {model.Email}. Please check your inbox.";
            return RedirectToAction(nameof(VerifyOTP), new { email = model.Email.Trim() });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Failed to send email: {ex.Message}");
            return View("ForgotPassword", model);
        }
    }

    [HttpGet]
    public IActionResult VerifyOTP(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new VerifyOtpViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyOTP(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string normalizedEmail = model.Email.Trim().ToLower();
        string cacheKey = $"OTP_{normalizedEmail}";

        if (_cache.TryGetValue(cacheKey, out string? cachedOtp) && cachedOtp == model.Otp.Trim())
        {
            _cache.Remove(cacheKey);
            string resetToken = Guid.NewGuid().ToString("N");
            string resetTokenKey = $"ResetAllowed_{normalizedEmail}";
            _cache.Set(resetTokenKey, resetToken, TimeSpan.FromMinutes(10));

            TempData["SuccessMessage"] = "OTP verified successfully! Please set your new password.";
            return RedirectToAction(nameof(SetNewPassword), new { email = model.Email.Trim(), token = resetToken });
        }

        ModelState.AddModelError(nameof(model.Otp), "Invalid or expired OTP code. Please check your email or request a new code.");
        return View(model);
    }

    [HttpGet]
    public IActionResult SetNewPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Invalid request. Please start the password reset process again.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        string normalizedEmail = email.Trim().ToLower();
        string resetTokenKey = $"ResetAllowed_{normalizedEmail}";

        if (!_cache.TryGetValue(resetTokenKey, out string? cachedToken) || cachedToken != token)
        {
            TempData["ErrorMessage"] = "Password reset session has expired or is invalid. Please request a new OTP.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new SetNewPasswordViewModel { Email = email.Trim(), Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetNewPassword(SetNewPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string normalizedEmail = model.Email.Trim().ToLower();
        string resetTokenKey = $"ResetAllowed_{normalizedEmail}";

        if (!_cache.TryGetValue(resetTokenKey, out string? cachedToken) || cachedToken != model.Token)
        {
            TempData["ErrorMessage"] = "Password reset session has expired. Please request a new OTP.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.email.ToLower() == normalizedEmail);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Login));
        }
        user.passwordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();
        _cache.Remove(resetTokenKey);

        TempData["SuccessMessage"] = "Your password has been reset successfully! You can now sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }
}

