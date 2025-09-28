using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrdersService.Data;

public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

        var host = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
        var db = Environment.GetEnvironmentVariable("ORDERS_DB") ?? "OrdersDb";
        var user = "sa";
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Danu1985";

        var connectionString = $"Server={host},{port};Database={db};User Id={user};Password={password};TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new OrdersDbContext(optionsBuilder.Options);
    }
}
