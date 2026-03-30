using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Galaxium.API.Data;

public class DesignTimeDbContextFactory
{
    public GalaxiumDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length > 0 
            ? args[0] 
            : "Host=localhost;Port=5432;Database=galaxium_bd;Username=sa;Password=galaxium_dev";

        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GalaxiumDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GalaxiumDbContext(optionsBuilder.Options);
    }
}
