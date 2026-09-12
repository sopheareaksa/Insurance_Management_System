using InsuranceManagement.Models;
using Insurance_Management_System.Models;

namespace Insurance_Management_System.ViewModels;

public class DashboardViewModel
{
    public int CustomerCount { get; set; }
    public int CompanyCount { get; set; }
    public int ProductCount { get; set; }
    public int InsuranceTypeCount { get; set; }
    public int PendingCompanyCount { get; set; }
    public IReadOnlyList<Company> PendingCompanies { get; set; } = [];
    public IReadOnlyList<InsuranceProduct> RecentProducts { get; set; } = [];
}