using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Insurance_Management_System.Models;
using Insurance_Management_System.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
    });

builder.Services.AddDbContext<InsuranceContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceContext>();
    var admin = await context.UserAccounts.FirstOrDefaultAsync(user => user.email == "admin@gmail.com");

    if (admin == null)
    {
        context.UserAccounts.Add(new UserAccount
        {
            username = "admin",
            email = "admin@gmail.com",
            passwordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
            role = "Admin",
            status = "Active",
            createdAt = DateTime.UtcNow
        });
    }

    var adminPasswordIsValid = false;
    if (admin != null)
    {
        try
        {
            adminPasswordIsValid = BCrypt.Net.BCrypt.Verify("12345", admin.passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            adminPasswordIsValid = false;
        }
    }

    if (admin != null && !adminPasswordIsValid)
    {
        admin.passwordHash = BCrypt.Net.BCrypt.HashPassword("12345");
        admin.role = "Admin";
        admin.status = "Active";
    }

    var defaultInsuranceTypes = new[]
    {
        new InsuranceType { Name = "Health Insurance", Description = "Coverage for medical care, treatment, and hospital expenses." },
        new InsuranceType { Name = "Life Insurance", Description = "Financial protection for beneficiaries after the policyholder's death." },
        new InsuranceType { Name = "Auto Insurance", Description = "Coverage for vehicles, accidents, and related liability." },
        new InsuranceType { Name = "Home Insurance", Description = "Protection for homes and personal property against covered losses." },
        new InsuranceType { Name = "Travel Insurance", Description = "Coverage for eligible travel emergencies and cancellations." }
    };

    foreach (var insuranceType in defaultInsuranceTypes)
    {
        if (!await context.InsuranceTypes.AnyAsync(type => type.Name == insuranceType.Name))
        {
            context.InsuranceTypes.Add(insuranceType);
        }
    }

    await context.SaveChangesAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();

