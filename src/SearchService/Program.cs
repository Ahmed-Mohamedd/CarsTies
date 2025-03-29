
using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>();//.AddPolicyHandler(GetPolicy());

var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton(new RedisService(redisConnection));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host("rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});



// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogRequestLogging();
app.MapControllers();

//await DbInitializer.InitDb(app);
//app.Lifetime.ApplicationStarted.Register(async () =>
//{
//    await Policy.Handle<TimeoutException>()
//        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
//        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
//});

app.Run();
//static IAsyncPolicy<HttpResponseMessage> GetPolicy()
//    => HttpPolicyExtensions
//        .HandleTransientHttpError()
//        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
//        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(10));
