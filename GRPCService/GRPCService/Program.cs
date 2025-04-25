using Confluent.Kafka;
using GRPCService.Producers;
using GRPCService.Services;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7168, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});


builder.Services.AddGrpc();
builder.Services.AddScoped<NumberMessageProducer>();
builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor >();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>
    
        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("GRPCService"))
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
        rider.AddProducer<NumberMessage>("number");
        rider.UsingKafka((context, k) =>
        {
            k.Host("kafka:9092");
            k.Acks = Acks.All;
        });
    });
});

var app = builder.Build();

// **Apply EF Core migrations at startup**  
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();    // creates DB file (if missing) and applies all pending migrations :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1} :contentReference[oaicite:2]{index=2}
}

// Configure the HTTP request pipeline.
app.MapGrpcService<SumService>();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();