using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Data;
using InventoryService.Models;
using InventoryService.Services;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // exige JWT por padrão
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        // GET: api/Products
        [HttpGet]
        [AllowAnonymous] // público para consulta de catálogo
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // POST: api/Products  (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Product p)
        {
            var created = await _service.CreateAsync(p);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT: api/Products/{id} (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Product p)
        {
            if (id != p.Id) return BadRequest();
            var ok = await _service.UpdateAsync(p);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/Products/{id} (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
