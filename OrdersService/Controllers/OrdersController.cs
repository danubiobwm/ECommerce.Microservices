using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.Models;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;

        public OrdersController(OrdersDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        [Authorize(Roles = "Admin")] // só Admin pode ver todos
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
                return BadRequest();

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // só Admin pode deletar
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
