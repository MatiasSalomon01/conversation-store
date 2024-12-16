using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WebApi.Configuration.Logging;
using WebApi.Configuration.RabbitMQ;
using WebApi.Constants;

var builder = WebApplication
    .CreateBuilder(args)
    .ConfigureLogs();

builder.Configuration
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();

var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApi(configuration)
                .AddMongoDB(configuration)
                .AddRabbitMQ(configuration);

builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString: configuration["MessageBroker:Host"]!,
                 name: "rabbitmq",
                 tags: new[] { "ready", "live" })
    .AddMongoDb(mongodbConnectionString: configuration["Database:ConnectionString"]!,
                name: "mongodb",
                tags: new[] { "ready", "live" });

var app = builder.Build();

app.LogEnviromentVariables(configuration);

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UsePathBase("/" + Constant.BasePrefix);

//Endpoints
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();