using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Insurance_Management_System.Models;
using Insurance_Management_System.ViewModels;
using Insurance_Management_System.DTOs;

namespace Insurance_Management_System.Controllers;

public class AuthController : Controller
{
    private readonly InsuranceContext _context;

    public AuthController(InsuranceContext context)
    {
        _context = context;
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
            return RedirectToAction("Index", "Home");
        }

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.email == email || u.username == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.passwordHash))
        {
            TempData["ErrorMessage"] = "Invalid credentials. Please verify your email/username and password.";
            return RedirectToAction("Index", "Home");
        }

        if (user.status != "Active")
        {
            TempData["ErrorMessage"] = $"Account status is '{user.status}'. Please contact administrator.";
            return RedirectToAction("Index", "Home");
        }

        user.lastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Welcome back, {user.username}! Signed in as {user.role}.";
        return RedirectToAction("Index", "Home");
    }
}

