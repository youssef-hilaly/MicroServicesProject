using GRPCService.Producers;
using Microsoft.EntityFrameworkCore;

namespace GRPCService.Services;

public interface IOutboxProcessor
{
    Task ProcessMessage(Guid messageId);
}
public class OutboxProcessor: IOutboxProcessor
{
    private readonly ILogger<OutboxProcessor> _logger;
    private IServiceScopeFactory _serviceScopeFactory;
    public OutboxProcessor(ILogger<OutboxProcessor> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ProcessMessage(Guid messageId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var producer = scope.ServiceProvider.GetRequiredService<NumberMessageProducer>();

        var message = await dbContext.OutboxMessages
            .FindAsync(messageId);
        
        if (message == null)
        {
            _logger.LogWarning("Outbox message {Id} not found", messageId);
            return;
        }
        
        try
        {
            var result = await producer.ProduceAsync(message.Number);
            
            if (result)
            {
                message.IsPublished = true;
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Published outbox message {Id}", message.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message {Id}", message.Id);
        }
    }
}
