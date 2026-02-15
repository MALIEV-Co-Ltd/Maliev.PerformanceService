using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Infrastructure.BackgroundServices;
using Maliev.PerformanceService.Infrastructure.Clients;
using Maliev.PerformanceService.Infrastructure.Consumers;
using Maliev.PerformanceService.Infrastructure.Data;
using Maliev.PerformanceService.Infrastructure.IAM;
using Maliev.PerformanceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Program.Log.StartingHost(bootstrapLogger, "Performance Service");

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
    builder.AddStandardCache("performance:"); // Redis + in-memory fallback, memory-optimized

    // --- 4. Messaging ---
    builder.AddMassTransitWithRabbitMq(x =>
    {
        x.AddConsumer<EmployeeCreatedEventConsumer>();
        x.AddConsumer<EmployeeTerminatedEventConsumer>();
    });

    // --- 5. Security ---
    builder.AddJwtAuthentication();

    // IAM Registration
    builder.AddIAMServiceClient("performance");
    builder.Services.AddIAMRegistration<PerformanceIAMRegistrationService>("performance");

    builder.Services.AddDataProtection();

    // --- 6. API Configuration ---
    builder.AddStandardCors(); // CORS with fail-fast validation
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
    builder.AddServiceClient("IAMService");

    // 9. Background Services
    builder.Services.AddHostedService<PerformanceReviewReminderBackgroundService>();
    builder.Services.AddHostedService<PIPCheckInReminderBackgroundService>();
    builder.Services.AddHostedService<DataArchivalBackgroundService>();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- 10. Database Migrations ---
    await app.MigrateDatabaseAsync<PerformanceDbContext>();

    // --- 11. Middleware Pipeline ---
    app.UseStandardMiddleware();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    // --- 12. Endpoints ---
    app.MapControllers();
    app.MapDefaultEndpoints(servicePrefix: "performance");
    app.MapApiDocumentation(servicePrefix: "performance");

    Program.Log.ServiceStarted(logger, "Performance Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Program.Log.HostTerminated(bootstrapLogger, ex, "Performance Service");
    // Force flush to ensure Aspire captures the error before process exits
    Console.Out.Flush();
    Console.Error.Flush();
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for the Performance Management Service.
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);
    }
}
