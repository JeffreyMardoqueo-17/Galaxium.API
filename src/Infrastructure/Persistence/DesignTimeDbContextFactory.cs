using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Galaxium.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GalaxiumDbContext>
{
    public GalaxiumDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Host=localhost;Port=5432;Database=galaxium_bd;Username=postgres;Password=galaxium_dev";

        var optionsBuilder = new DbContextOptionsBuilder<GalaxiumDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GalaxiumDbContext(optionsBuilder.Options);
    }
}