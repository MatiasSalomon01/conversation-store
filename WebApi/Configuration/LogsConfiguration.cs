using Serilog;
using Serilog.Formatting.Compact;

namespace WebApi.Configuration.Logging;

public static class LogsConfiguration
{
    public static WebApplicationBuilder ConfigureLogs(this WebApplicationBuilder builder)
    {
        Serilog.Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        Serilog.Log.Information("Starting ms-clt-conversation-store");

        builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
             .Enrich.FromLogContext()
             .ReadFrom.Configuration(hostingContext.Configuration));

        return builder;
    }
    public static void LogEnviromentVariables(this WebApplication app, IConfiguration configuration)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Variables de entorno");

        foreach (var keyValue in configuration.GetChildren().ToList())
        {
            if (keyValue.Key == "Database" || keyValue.Key == "MessageBroker" || keyValue.Key == "InitialResponseWith")
            {
                if (keyValue.Value is null)
                {
                    foreach (var child in configuration.GetSection(keyValue.Key).AsEnumerable())
                    {
                        logger.LogInformation("Key: {key}, Value: {value}", child.Key, child.Value);
                    }
                }
                else
                {
                    logger.LogInformation("Key: {key}, Value: {value}", keyValue.Key, keyValue.Value);
                }
            }
        }
    }
}
