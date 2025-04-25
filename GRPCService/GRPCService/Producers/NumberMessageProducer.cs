using MassTransit;

namespace GRPCService.Producers;

public class NumberMessage
{
    public int Number { get; set; }
}

public class NumberMessageProducer
{
    private readonly ITopicProducer<NumberMessage> _producer;
    private readonly ILogger<NumberMessageProducer> _logger;

    public NumberMessageProducer(
        ITopicProducer<NumberMessage> producer,
        ILogger<NumberMessageProducer> logger)
    {
        _producer = producer;
        _logger = logger;
    }
    
    public async Task<bool> ProduceAsync(int number)
    {
        _logger.LogInformation("Producing NumberMessage to Kafka topic 'number'");
        try
        {
            await _producer.Produce(new NumberMessage { Number = number });
            _logger.LogInformation("Produced NumberMessage {{ Number = {Number} }}", number);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing NumberMessage");
            return false;
        }
    }
}