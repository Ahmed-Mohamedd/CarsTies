
using System.Net;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//builder.Services.AddHttpClient<AuctionServiceHttpClient>();//.AddPolicyHandler(GetPolicy());

var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton(new RedisService(redisConnection));

// configure serilog.
builder.Host.UseSerilog((context, services, configuration) =>
{
    // Reads configuration settings for Serilog from the appsettings.json file or any other configuration source
    // This enables setting options such as log levels, sinks, and output formats directly from configuration files.
    configuration.ReadFrom.Configuration(context.Configuration);
    // Integrate with the dependency injection container, enabling sinks to use other registered services.
    // This is useful if any of the logging sinks require dependencies such as database or HTTP context.
    configuration.ReadFrom.Services(services);
});

// add health checks for redis.
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis", tags: new[] { "redis" });


// configure massTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false));
    x.UsingRabbitMq((context, cfg) =>
    {

        //cfg.Host("rabbitmq://localhost");
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(6,6));

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});



builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

// add health checks ui to the app.
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();












//await DbInitializer.InitDb(app);
//app.Lifetime.ApplicationStarted.Register(async () =>
//{
//    await Policy.Handle<TimeoutException>()
//        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
//        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
//});

//static IAsyncPolicy<HttpResponseMessage> GetPolicy()
//    => HttpPolicyExtensions
//        .HandleTransientHttpError()
//        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
//        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(10));
