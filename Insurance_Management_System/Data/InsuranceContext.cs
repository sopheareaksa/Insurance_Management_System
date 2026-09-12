using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserAccount>().ToTable("UserAccount");
        modelBuilder.Entity<Customer>().ToTable("Customer");
        modelBuilder.Entity<Company>().ToTable("Company");
    }
}