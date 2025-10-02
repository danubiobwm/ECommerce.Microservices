using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersService.DTOs;
using OrdersService.Services;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _service;

        public OrdersController(OrderService service)
        {
            _service = service;
        }

        // Criar pedido (user ou admin)
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result.Order);
        }

        // Listar pedidos (apenas admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllAsync();
            return Ok(orders);
        }

        // Buscar pedido por ID (user/admin)
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _service.GetByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }
    }
}
