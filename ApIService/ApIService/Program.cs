using APIService.Consumers;
using APIService.Services;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace APIService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Add services to the container.
        builder.Services.AddScoped<INumberService, NumberService>();
        builder.Services.AddSingleton(new SemaphoreSlim(1, 1));
        
        builder.Services.AddOpenTelemetry()
            .WithMetrics(opt =>
        
                opt
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("APIService"))
                    .AddMeter(builder.Configuration.GetValue<string>("OpenRemoteManageMeterName"))
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"]);
                    })
            );

        builder.Services.AddMassTransit(x =>
        {
            x.UsingInMemory();
            x.AddRider(rider =>
            {
                rider.AddConsumer<NumberConsumer>();
                rider.UsingKafka((context, k) =>
                {
                    k.Host("kafka:9092");
                    k.TopicEndpoint<NumberMessage>("number", "number-consumer-group", e =>
                    {
                        e.ConfigureConsumer<NumberConsumer>(context);
                        e.CreateIfMissing(r => { r.NumPartitions = 1; r.ReplicationFactor = 1; });
                    });
                });
            });
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}