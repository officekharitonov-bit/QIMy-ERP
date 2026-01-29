using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using QIMy.Application.Common.Behaviours;
using QIMy.Application.Common.Interfaces;
using QIMy.Infrastructure.Data;
using QIMy.Infrastructure.Repositories;
using QIMy.Infrastructure.Services;
using QIMy.Core.Interfaces;
using QIMy.API.Filters;
using QIMy.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Database (Azure SQL / SQLite)
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

// Multi-tenant accessor (used by EF Core global filters + SaveChanges guardrails)
builder.Services.AddScoped<ICurrentBusinessIdAccessor, CurrentBusinessIdAccessor>();

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

// Add HttpClient for Vatlayer API
builder.Services.AddHttpClient<IVatlayerService, VatlayerService>();

// Add Vatlayer service
builder.Services.AddScoped<IVatlayerService, VatlayerService>();

// Add VAT rate update background service (runs daily)
builder.Services.AddHostedService<VatRateUpdateService>();

// Add legacy services for backward compatibility (import/export)
builder.Services.AddScoped<IClientService, ClientService>();

// Add Personen Index import service
builder.Services.AddScoped<PersonenIndexImportService>();

// Add Duplicate Detection service
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwashbuckleFileUploadFilter>();
});
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Resolve tenant (BusinessId) for each request (query string or header)
app.UseMiddleware<BusinessIdAccessorMiddleware>();

app.MapControllers();

// Seed reference data on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var businessAccessor = scope.ServiceProvider.GetRequiredService<ICurrentBusinessIdAccessor>();

        // Startup tasks must see all tenants.
        businessAccessor.BypassTenantFilter = true;
        // Apply pending migrations (if present) and ensure schema is created (SQLite dev runs may have empty migrations)
        context.Database.Migrate();
        await context.Database.EnsureCreatedAsync();
        await QIMy.Infrastructure.Data.SeedData.SeedReferenceData(context);
        Console.WriteLine("✅ Database migrated and seeded successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error with database: {ex.Message}");
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
