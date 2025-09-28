using InventoryService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryService.Messaging;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inventory Consumer iniciado (RabbitMQ desativado temporariamente)");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Simulando processamento de pedidos...");
            await Task.Delay(10000, stoppingToken);
        }
    }
}