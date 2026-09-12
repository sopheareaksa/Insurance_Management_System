using Microsoft.EntityFrameworkCore;
using InsuranceManagement.Models;

namespace Insurance_Management_System.Models;

public class InsuranceContext : DbContext
{
    public InsuranceContext(DbContextOptions<InsuranceContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<InsuranceType> InsuranceTypes { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<InsuranceProduct> InsuranceProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserAccount>().ToTable("UserAccount");
        modelBuilder.Entity<Customer>().ToTable("Customer");
        modelBuilder.Entity<Company>().ToTable("Company");
        modelBuilder.Entity<InsuranceType>().ToTable("InsuranceType");
        modelBuilder.Entity<InsuranceProduct>().ToTable("InsuranceProduct");
    }
}