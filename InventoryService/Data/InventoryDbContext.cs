using Microsoft.EntityFrameworkCore;
using InventoryService.Models;

namespace InventoryService.Data;
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> opts) : base(opts) { }
    public DbSet<Product> Products { get; set; } = null!;
}
