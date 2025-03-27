using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await DbInitializer.InitDb(app);
//app.Lifetime.ApplicationStarted.Register(async () =>
//{
//    await Policy.Handle<TimeoutException>()
//        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
//        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
//});


app.Run();
