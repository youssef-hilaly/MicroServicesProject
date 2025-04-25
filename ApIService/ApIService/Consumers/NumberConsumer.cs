using APIService.Services;
using MassTransit;

namespace APIService.Consumers;

public class NumberMessage
{
    public int Number { get; set; }
}
public class NumberConsumer : IConsumer<NumberMessage>
{
    private readonly ILogger<NumberConsumer> _logger;
    private readonly INumberService _numberService;
    
    public NumberConsumer(ILogger<NumberConsumer> logger, INumberService numberService)
    {
        _numberService = numberService;
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<NumberMessage> context)
    {
        _numberService.SetNumber(context.Message.Number);
        _logger.LogInformation($"Consumed number: {context.Message.Number}");
        return Task.CompletedTask;
    }
}