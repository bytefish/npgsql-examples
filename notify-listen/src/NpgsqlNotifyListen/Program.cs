using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlNotifyListen.Infrastructure;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

// We will log to %LocalAppData%/GitClub to store the Logs, so it doesn't need to be configured 
// to a different path, when you run it on your machine.
string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitClub");

// We are writing with RollingFileAppender using a daily rotation, and we want to have the filename as 
// as "GitClub-{Date}.log", the date will be set by Serilog automagically.
string logFilePath = Path.Combine(logDirectory, "GitClub-.log");

// Configure the Serilog Logger. This Serilog Logger will be passed 
// to the Microsoft.Extensions.Logging LoggingBuilder using the 
// LoggingBuilder#AddSerilog(...) extension.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>();

    // Logging
    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

    // Database
    builder.Services.AddSingleton<NpgsqlDataSource>((sp) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ApplicationDatabase");

        if (connectionString == null)
        {
            throw new InvalidOperationException("No ConnectionString named 'ApplicationDatabase' was found");
        }

        // Since version 7.0, NpgsqlDataSource is the recommended way to use Npgsql. When using NpsgqlDataSource,
        // NodaTime currently has to be configured twice - once at the EF level, and once at the underlying ADO.NET
        // level (there are plans to improve this):
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        return dataSourceBuilder.Build();
    });

    // Handles incoming Postgres Notifications
    builder.Services.AddSingleton<IPostgresNotificationHandler, LoggingPostgresNotificationHandler>();

    // Add Notification Service
    builder.Services.Configure<PostgresNotificationServiceOptions>(o => o.ChannelName = "core_db_event");

    builder.Services.AddHostedService<PostgresNotificationService>();

    IHost host = builder.Build();

    host.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "An unhandeled exception occured.");
}
finally
{
    // Wait 0.5 seconds before closing and flushing, to gather the last few logs.
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    await Log.CloseAndFlushAsync();
}