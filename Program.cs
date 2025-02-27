using SokeBot;
using SokeBot.DataModel.RiotApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<BotMain, BotMain>();
builder.Services.AddTransient<RiotApi, RiotApi>();
builder.Services.AddSingleton<RiotPlayerWatcher, RiotPlayerWatcher>();
builder.Services.AddDbContext<BotDb>(ServiceLifetime.Singleton);
builder.Services.AddTransient<DataLogger, DataLogger>();
builder.Services.AddTransient<DatabaseOperations, DatabaseOperations>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BotDb>().Database.EnsureCreated();
}
app.Services.GetRequiredService<BotMain>().Start(() => app.Services.GetRequiredService<RiotPlayerWatcher>().WatchPlayers());

app.Run();
