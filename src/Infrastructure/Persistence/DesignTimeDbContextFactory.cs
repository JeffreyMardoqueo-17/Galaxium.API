using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Galaxium.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GalaxiumDbContext>
{
    public GalaxiumDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=galaxium_erp;Username=galaxium;Password=GalaxiumDev2026!";

        var optionsBuilder = new DbContextOptionsBuilder<GalaxiumDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GalaxiumDbContext(optionsBuilder.Options);
    }
}
