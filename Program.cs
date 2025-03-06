using SokeBot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BotMain, BotMain>();
builder.Services.AddTransient<RiotApi, RiotApi>();
builder.Services.AddSingleton<RiotPlayerWatcher, RiotPlayerWatcher>();
builder.Services.AddDbContext<BotDb>(ServiceLifetime.Singleton);
builder.Services.AddTransient<DataLogger, DataLogger>();
builder.Services.AddTransient<DatabaseOperations, DatabaseOperations>();
builder.Services.AddTransient<FileLogger, FileLogger>();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<BotDb>().Database.EnsureCreated();
    }
    app.Services.GetRequiredService<BotMain>().Start(() => app.Services.GetRequiredService<RiotPlayerWatcher>().StartWatcher());

    app.Run();
}
catch(Exception e)
{
    var logger = new FileLogger();
    logger.Log(e.ToString());
}

