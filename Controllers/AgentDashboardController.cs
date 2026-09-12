using System.Security.Claims;
using Insurance_Management_System.Models;
using Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Management_System.Controllers;

[Authorize(Roles = "Agent")]
public class AgentDashboardController : Controller
{
    private readonly InsuranceContext _context;

    public AgentDashboardController(InsuranceContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var company = await _context.Companies.FirstOrDefaultAsync(item => item.UserId == userId);
        if (company == null)
        {
            return NotFound("The agent company profile was not found.");
        }

        var products = await _context.InsuranceProducts
            .Where(product => product.CompanyId == company.CompanyId)
            .Include(product => product.InsuranceType)
            .OrderByDescending(product => product.InProId)
            .ToListAsync();

        return View(new AgentDashboardViewModel
        {
            Company = company,
            ProductCount = products.Count,
            Products = products
        });
    }
}