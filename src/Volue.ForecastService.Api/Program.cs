using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using Volue.ForecastService.Api.Middleware;
using Volue.ForecastService.Repositories;
using Volue.ForecastService.Services;
using Scalar.AspNetCore;
// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Starting Volue Forecast Service API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the application
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

// Configure Database
builder.Services.AddDbContext<ForecastDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        )
    )
);

// Register Repositories
builder.Services.AddRepositories();

// Register Application Services (with event publishing)
builder.Services.AddApplicationServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "postgres" });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS (if needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ForecastDbContext>();
    try
    {
        Log.Information("Applying database migrations...");
        dbContext.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
        throw;
    }
}

// Configure the HTTP request pipeline.

// Correlation ID middleware (early in pipeline to track all requests)
app.UseCorrelationId();

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.GetCorrelationId());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

// Global exception handling (after correlation ID)
app.UseExceptionHandlingMiddleware();

// Enable OpenAPI/Swagger in Development and Docker environments
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Modern Swagger UI alternative
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Make Program class accessible for integration tests
public partial class Program { }
