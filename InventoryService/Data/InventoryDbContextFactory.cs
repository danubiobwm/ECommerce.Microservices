using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryService.Data;

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
        var db = Environment.GetEnvironmentVariable("INVENTORY_DB") ?? "InventoryDb";
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "P@ssw0rd123!";

        var connectionString = $"Server={host},{port};Database={db};User Id=sa;Password={password};TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);
        return new InventoryDbContext(optionsBuilder.Options);
    }
}
