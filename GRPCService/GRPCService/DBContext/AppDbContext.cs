using Microsoft.EntityFrameworkCore;
using OutboxMessage = GRPCService.Models.OutboxMessage;

public class AppDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }   
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}