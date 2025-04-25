namespace GRPCService.Models;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Number { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
