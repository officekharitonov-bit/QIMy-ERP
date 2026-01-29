using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Web.Services;

public class DbContextFactoryWrapper : IDbContextFactory<ApplicationDbContext>
{
    private readonly string _connectionString;
    private readonly bool _isDevelopment;
    private readonly IServiceProvider _serviceProvider;

    public DbContextFactoryWrapper(string connectionString, bool isDevelopment, IServiceProvider serviceProvider)
    {
        _connectionString = connectionString;
        _isDevelopment = isDevelopment;
        _serviceProvider = serviceProvider;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (_isDevelopment)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
        else
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        // Get the business ID accessor from service provider
        var businessIdAccessor = _serviceProvider.GetRequiredService<ICurrentBusinessIdAccessor>();

        return new ApplicationDbContext(optionsBuilder.Options, businessIdAccessor);
    }
}
