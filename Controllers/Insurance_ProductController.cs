using Insurance_Management_System.Models;
using InsuranceManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Insurance_Management_System.Controllers
{
    public class InsuranceProductController : Controller
    {
        private readonly InsuranceContext _context;

        public InsuranceProductController(InsuranceContext context)
        {
            _context = context;
        }

        // GET: InsuranceProduct
        public async Task<IActionResult> Index()
        {
            var items = await _context.InsuranceProducts
                .Include(p => p.Company)
                .Include(p => p.InsuranceType)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(items);
        }

        // GET: InsuranceProduct/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.InsuranceProducts
                .Include(p => p.Company)
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.InProId == id.Value);

            if (product == null) return NotFound();
            return View(product);
        }

        // GET: InsuranceProduct/Create
        public IActionResult Create()
        {
            PopulateDropDowns();
            return View();
        }

        // POST: InsuranceProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyId,InTypeId,Name,BasePremium,Detail,Status")] InsuranceProduct model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns(model.CompanyId, model.InTypeId);
                return View(model);
            }

            try
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                ModelState.AddModelError(string.Empty, "Database error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product: " + ex.Message);
            }

            PopulateDropDowns(model.CompanyId, model.InTypeId);
            return View(model);
        }

        // GET: InsuranceProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.InsuranceProducts.FindAsync(id.Value);
            if (product == null) return NotFound();

            PopulateDropDowns(product.CompanyId, product.InTypeId);
            return View(product);
        }

        // POST: InsuranceProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InProId,CompanyId,InTypeId,Name,BasePremium,Detail,Status")] InsuranceProduct model)
        {
            if (id != model.InProId) return BadRequest();

            if (!ModelState.IsValid)
            {
                PopulateDropDowns(model.CompanyId, model.InTypeId);
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.InsuranceProducts.AnyAsync(e => e.InProId == model.InProId))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                ModelState.AddModelError(string.Empty, "Database error: " + sqlEx.Message);
            }

            PopulateDropDowns(model.CompanyId, model.InTypeId);
            return View(model);
        }

        // GET: InsuranceProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.InsuranceProducts
                .Include(p => p.Company)
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.InProId == id.Value);

            if (product == null) return NotFound();
            return View(product);
        }

        // POST: InsuranceProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.InsuranceProducts.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                _context.InsuranceProducts.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                ModelState.AddModelError(string.Empty, "Database error: " + sqlEx.Message);
                return View(product);
            }
        }

        // Helper method to load foreign key SelectLists for dropdowns
        private void PopulateDropDowns(object? selectedCompany = null, object? selectedType = null)
        {
            ViewBag.CompanyId = new SelectList(_context.Set<Company>(), "CompanyId", "Name", selectedCompany);
            ViewBag.InTypeId = new SelectList(_context.Set<InsuranceType>(), "InTypeId", "Name", selectedType);
        }
    }
}