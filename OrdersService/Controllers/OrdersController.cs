using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersService.DTOs;
using OrdersService.Services;
using OrdersService.Data;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrdersDbContext _db;
    public OrdersController(OrderService orderService, OrdersDbContext db) { _orderService = orderService; _db = db; }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var items = dto.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var (success, msg, order) = await _orderService.CreateOrderAsync(items);
        if (!success) return BadRequest(new { message = msg });
        return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return Ok(order);
    }
}
