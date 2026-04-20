using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Automotive.Marketplace.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutomotiveContext>
{
    public AutomotiveContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AutomotiveContext>()
            .UseNpgsql("Host=localhost;Database=automotive_migrations_design;Username=postgres;Password=postgres")
            .UseLazyLoadingProxies()
            .Options;

        return new AutomotiveContext(options);
    }
}
