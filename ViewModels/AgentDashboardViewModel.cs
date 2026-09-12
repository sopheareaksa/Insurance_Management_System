using InsuranceManagement.Models;
using Insurance_Management_System.Models;

namespace Insurance_Management_System.ViewModels;

public class AgentDashboardViewModel
{
    public Company Company { get; set; } = null!;
    public int ProductCount { get; set; }
    public IReadOnlyList<InsuranceProduct> Products { get; set; } = [];
}