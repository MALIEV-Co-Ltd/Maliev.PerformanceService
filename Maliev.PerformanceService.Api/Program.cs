using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Infrastructure.BackgroundServices;
using Maliev.PerformanceService.Infrastructure.Clients;
using Maliev.PerformanceService.Infrastructure.Data;
using Maliev.PerformanceService.Infrastructure.IAM;
using Maliev.PerformanceService.Infrastructure.Repositories;
using Maliev.PerformanceService.Infrastructure.Consumers;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume();

// --- 2. Infrastructure & Observability ---
builder.AddServiceDefaults();
builder.AddStandardMiddleware(options =>
{
    options.EnableRequestLogging = true;
});
builder.AddServiceMeters("performance-service");

// --- 3. Data & Cache ---
builder.AddPostgresDbContext<PerformanceDbContext>(connectionName: "PerformanceDbContext");
builder.AddRedisDistributedCache(instanceName: "performance:");

// --- 4. Messaging ---
builder.AddMassTransitWithRabbitMq(x =>
{
    x.AddConsumer<EmployeeCreatedEventConsumer>();
    x.AddConsumer<EmployeeTerminatedEventConsumer>();
});

// --- 5. Security ---
builder.AddJwtAuthentication();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddIAMRegistration<PerformanceIAMRegistrationService>();
}
builder.Services.AddDataProtection();

// --- 6. API Configuration ---
builder.AddDefaultCors();
builder.AddDefaultApiVersioning();
builder.AddStandardRateLimiting();

if (!builder.Environment.IsProduction())
{
    builder.AddStandardOpenApi(
        title: "MALIEV Performance Management Service API",
        description: "Manages employee performance reviews, goals, and feedback.");
}

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
});

// --- 7. Application Services ---
builder.Services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IPIPRepository, PIPRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// Command/Query Handlers
builder.Services.AddScoped<CreatePerformanceReviewCommandHandler>();
builder.Services.AddScoped<UpdatePerformanceReviewCommandHandler>();
builder.Services.AddScoped<SubmitPerformanceReviewCommandHandler>();
builder.Services.AddScoped<AcknowledgePerformanceReviewCommandHandler>();
builder.Services.AddScoped<CreateGoalCommandHandler>();
builder.Services.AddScoped<UpdateGoalCommandHandler>();
builder.Services.AddScoped<UpdateGoalProgressCommandHandler>();
builder.Services.AddScoped<SubmitFeedbackCommandHandler>();
builder.Services.AddScoped<CreatePIPCommandHandler>();
builder.Services.AddScoped<UpdatePIPCommandHandler>();
builder.Services.AddScoped<RecordPIPOutcomeCommandHandler>();
builder.Services.AddScoped<GetPerformanceReviewsQueryHandler>();
builder.Services.AddScoped<GetPerformanceReviewByIdQueryHandler>();
builder.Services.AddScoped<GetGoalsQueryHandler>();
builder.Services.AddScoped<GetGoalByIdQueryHandler>();
builder.Services.AddScoped<GetFeedbackQueryHandler>();
builder.Services.AddScoped<GetPIPsQueryHandler>();

// Validators
builder.Services.AddScoped<Maliev.PerformanceService.Application.Validators.CreatePerformanceReviewValidator>();
builder.Services.AddScoped<Maliev.PerformanceService.Application.Validators.CreateGoalValidator>();
builder.Services.AddScoped<Maliev.PerformanceService.Application.Validators.UpdateGoalProgressValidator>();
builder.Services.AddScoped<Maliev.PerformanceService.Application.Validators.SubmitFeedbackValidator>();
builder.Services.AddScoped<Maliev.PerformanceService.Application.Validators.CreatePIPValidator>();

// 8. HTTP Clients
builder.AddServiceClient<IEmployeeServiceClient, EmployeeServiceClient>("EmployeeService");
builder.AddServiceClient<INotificationServiceClient, NotificationServiceClient>("NotificationService");
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.AddServiceClient("IAMService");
}

// 9. Background Services
builder.Services.AddHostedService<PerformanceReviewReminderBackgroundService>();
builder.Services.AddHostedService<PIPCheckInReminderBackgroundService>();
builder.Services.AddHostedService<DataArchivalBackgroundService>();

var app = builder.Build();

// --- 10. Database Migrations ---
try
{
    await app.MigrateDatabaseAsync<PerformanceDbContext>();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database migration failed");
}

// --- 11. Middleware Pipeline ---
app.UseStandardMiddleware();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// --- 12. Endpoints ---
app.MapControllers();
app.MapDefaultEndpoints(servicePrefix: "performance");
app.MapApiDocumentation(servicePrefix: "performance");

await app.RunAsync();

/// <summary>
/// Main program class for the Performance Management Service.
/// </summary>
public partial class Program { }
