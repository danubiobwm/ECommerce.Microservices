using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryService.DTOs;
using InventoryService.Models;
using InventoryService.Services;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) { _svc = svc; }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var list = await _svc.GetAll();
        var dto = list.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Quantity));
        return Ok(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _svc.Get(id);
        if (p == null) return NotFound();
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Quantity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto model)
    {
        var p = new Product { Name = model.Name, Description = model.Description, Price = model.Price, Quantity = model.Quantity };
        var created = await _svc.Create(p);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("check")]
    [Authorize]
    public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequest req)
    {
        var items = req.Items.Select(i => new Services.CheckItem(i.ProductId, i.Quantity)).ToList();
        var res = await _svc.CheckAvailability(items);
        return Ok(new CheckAvailabilityResponse(res.AllAvailable, res.UnavailableItems.Select(u => new CheckItemDto(u.ProductId, u.Quantity)).ToList()));
    }
}
