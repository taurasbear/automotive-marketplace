using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Automotive.Marketplace.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutomotiveContext>
{
    public AutomotiveContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AutomotiveContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=automotive_marketplace;Username=postgres;Password=postgres")
            .UseLazyLoadingProxies()
            .Options;

        return new AutomotiveContext(options);
    }
}
