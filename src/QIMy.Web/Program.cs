using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using System.Text;
using QIMy.Application.Common.Behaviours;
using QIMy.Application.Common.Interfaces;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;
using QIMy.Infrastructure.Repositories;
using QIMy.Infrastructure.Services;
using QIMy.Web.Components;
using QIMy.Web.Services;
using QIMy.AI; // AI Services

var builder = WebApplication.CreateBuilder(args);

// Enable Windows-1252 and other code pages used by CSV imports
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add Database (Azure SQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Add ASP.NET Core Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Add application services
// ============== NEW ARCHITECTURE ==============

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(QIMy.Application.Clients.Commands.CreateClient.CreateClientCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(QIMy.Application.Clients.Commands.CreateClient.CreateClientCommandValidator).Assembly);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(QIMy.Application.MappingProfiles.ClientProfile).Assembly);

// Add Repository Pattern + Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============== DUPLICATE DETECTION SERVICE ==============
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();

// ============== AI SERVICES ==============
builder.Services.AddAiServices();

// ============== OLD SERVICES (Keep for backward compatibility) ==============
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ClientImportService>();
builder.Services.AddScoped<ClientExportService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IViesService, ViesService>();
builder.Services.AddScoped<PdfInvoiceGeneratorService>();

// ============== TAX LOGIC ENGINE ==============
builder.Services.AddScoped<InvoiceTaxService>();
builder.Services.AddScoped<FinalReportService>();
builder.Services.AddScoped<AustrianInvoicePdfService>();
builder.Services.AddScoped<NumberingService>();

// ============== NEW DOCUMENT TYPE SERVICES ==============
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
builder.Services.AddScoped<BusinessContext>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add cascading authentication state
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Seed reference data on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Apply migrations properly (instead of EnsureCreatedAsync which bypasses migrations)
        await context.Database.MigrateAsync();
        
        // Seed reference data (currencies, tax rates, etc.)
        await QIMy.Infrastructure.Data.SeedData.SeedReferenceData(context);

        // Ensure default admin user exists and is not locked out (dev convenience)
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = "office@kharitonov.at";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Egor",
                LastName = "Kharitonov",
                Role = "Admin",
                IsActive = true
            };

            // Development default password
            var createResult = await userManager.CreateAsync(admin, "Admin123!");
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to create admin user: {errors}");
            }
            else
            {
                Console.WriteLine("✅ Admin user created successfully: office@kharitonov.at / Admin123!");
            }
        }
        else
        {
            // Reset lockout and password on each startup (dev convenience)
            await userManager.ResetAccessFailedCountAsync(admin);
            await userManager.SetLockoutEndDateAsync(admin, DateTimeOffset.UtcNow);

            // Reset password to default on each startup
            var token = await userManager.GeneratePasswordResetTokenAsync(admin);
            var resetResult = await userManager.ResetPasswordAsync(admin, token, "Admin123!");
            if (resetResult.Succeeded)
            {
                Console.WriteLine("✅ Admin password reset to: Admin123!");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error seeding data: {ex.Message}");
}

// Seed default businesses and link admin user to them for multi-business selector
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    var adminEmail = "office@kharitonov.at";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin != null)
    {
        var businessNames = new[] { "AKHA GmbH", "BKHA GmbH", "Mag. Kharitonov Egor" };
        var businesses = new List<Business>();

        foreach (var name in businessNames)
        {
            var biz = await context.Businesses.FirstOrDefaultAsync(b => b.Name == name && !b.IsDeleted);
            if (biz == null)
            {
                biz = new Business
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Businesses.Add(biz);
            }
            businesses.Add(biz);
        }

        await context.SaveChangesAsync();

        var defaultName = "AKHA GmbH";
        foreach (var biz in businesses)
        {
            var exists = await context.UserBusinesses
                .AnyAsync(ub => ub.UserId == admin.Id && ub.BusinessId == biz.Id && !ub.IsDeleted);
            if (!exists)
            {
                context.UserBusinesses.Add(new UserBusiness
                {
                    UserId = admin.Id,
                    BusinessId = biz.Id,
                    Role = "Owner",
                    IsDefault = biz.Name == defaultName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }

        // Set default BusinessId on admin user
        var defaultBiz = businesses.FirstOrDefault(b => b.Name == defaultName);
        if (defaultBiz != null)
        {
            admin.BusinessId = defaultBiz.Id;
            context.Update(admin);
        }

        await context.SaveChangesAsync();

        // Bind all existing data (Invoices, Quotes) to AKHA GmbH
        var akha = businesses.FirstOrDefault(b => b.Name == "AKHA GmbH");
        if (akha != null)
        {
            // Update Invoices without BusinessId
            var invoicesWithoutBusiness = await context.Invoices.Where(i => i.BusinessId == null && !i.IsDeleted).ToListAsync();
            foreach (var invoice in invoicesWithoutBusiness)
            {
                invoice.BusinessId = akha.Id;
            }

            // Update Quotes without BusinessId
            var quotesWithoutBusiness = await context.Quotes.Where(q => q.BusinessId == null && !q.IsDeleted).ToListAsync();
            foreach (var quote in quotesWithoutBusiness)
            {
                quote.BusinessId = akha.Id;
            }

            if (invoicesWithoutBusiness.Count > 0 || quotesWithoutBusiness.Count > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"Linked {invoicesWithoutBusiness.Count} invoices, {quotesWithoutBusiness.Count} quotes to AKHA GmbH");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error seeding businesses: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();






