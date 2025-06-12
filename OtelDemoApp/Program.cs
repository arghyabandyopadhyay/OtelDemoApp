using OpenTelemetry.Metrics;
using OpenTelemetry.Trace; // Add tracing
using OpenTelemetry.Logs;  // Add logging
using System.Diagnostics.Metrics;
using System.Diagnostics;   // For ActivitySource
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://collector:4317";

// Add OpenTelemetry Metrics, Tracing, and Logging
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation() // HTTP server metrics
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()        // HTTP client metrics
        .AddProcessInstrumentation()
        .AddMeter("MyApplication")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("MyApplication")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    });

// Add OpenTelemetry logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.ParseStateValues = true;
    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(otlpEndpoint);
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Custom meter and counter for weatherforecast requests
var meter = new Meter("MyApplication");
var weatherCounter = meter.CreateCounter<int>("weatherforecast_requests", description: "Number of weather forecast requests");

// Custom ActivitySource for tracing
var activitySource = new ActivitySource("MyApplication");

// Get logger
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("WeatherForecastLogger");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    weatherCounter.Add(1); // Increment custom metric

    using var activity = activitySource.StartActivity("GenerateWeatherForecast");
    activity?.SetTag("custom.event", "weatherforecast_requested");

    logger.LogInformation("WeatherForecast endpoint called at {Time}", DateTime.UtcNow);

    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    logger.LogInformation("WeatherForecast generated: {@Forecast}", forecast);

    activity?.AddEvent(new ActivityEvent("WeatherForecastGenerated"));

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
