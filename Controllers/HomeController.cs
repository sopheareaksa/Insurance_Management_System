using Insurance_Management_System.Models;
using Insurance_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Insurance_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly InsuranceContext _context;

        public HomeController(InsuranceContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                CustomerCount = await _context.Customers.CountAsync(),
                CompanyCount = await _context.Companies.CountAsync(),
                ProductCount = await _context.InsuranceProducts.CountAsync(),
                InsuranceTypeCount = await _context.InsuranceTypes.CountAsync(),
                PendingCompanyCount = await _context.Companies.CountAsync(company => company.ApproveStatus == "Pending"),
                PendingCompanies = await _context.Companies
                    .Where(company => company.ApproveStatus == "Pending")
                    .OrderByDescending(company => company.CreatedAt)
                    .ToListAsync(),
                RecentProducts = await _context.InsuranceProducts
                    .Include(product => product.Company)
                    .Include(product => product.InsuranceType)
                    .OrderByDescending(product => product.InProId)
                    .Take(6)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveCompany(int id)
        {
            var company = await _context.Companies
                .Include(item => item.UserAccount)
                .FirstOrDefaultAsync(item => item.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }

            company.ApproveStatus = "Approved";
            if (company.UserAccount != null)
            {
                company.UserAccount.status = "Active";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{company.Name} has been approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectCompany(int id)
        {
            var company = await _context.Companies
                .Include(item => item.UserAccount)
                .FirstOrDefaultAsync(item => item.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }

            company.ApproveStatus = "Rejected";
            if (company.UserAccount != null)
            {
                company.UserAccount.status = "Inactive";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{company.Name} has been rejected.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
