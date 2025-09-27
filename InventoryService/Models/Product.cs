namespace InventoryService.Models;
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
