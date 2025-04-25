using Grpc.Core;
using GRPCService.Models;

namespace GRPCService.Services;

public class SumService : GRPCService.SumService.SumServiceBase
{
    private readonly ILogger<SumService> _logger;
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly AppDbContext _dbContext;

    public SumService(ILogger<SumService> logger, AppDbContext dbContext, IOutboxProcessor outboxProcessor)
    {
        _outboxProcessor = outboxProcessor;
        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<SumResponse> AddNumbers(SumRequest request, ServerCallContext context)
    {
        try
        {
            var sum = request.Number1 + request.Number2;
            
            _logger.LogInformation($"Sum of {request.Number1} and {request.Number2} is {sum}");
            
            var outboxMessage = new OutboxMessage()
            {
                IsPublished = false,
                Id = Guid.NewGuid(),
                Number = sum,
                CreatedAt = DateTime.Now
            };
            _dbContext.OutboxMessages.Add(outboxMessage);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _outboxProcessor.ProcessMessage(outboxMessage.Id);
            
            return await Task.FromResult(new SumResponse
            {
                Result = true,
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Sum method");
            return await Task.FromResult(new SumResponse
            {
                Result = false
            });
        }
    }
    
}