
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;
using PaquitoBot.Api;
using PaquitoBot.Api.HealthChecks;
using PaquitoBot.Api.Middleware;
using PaquitoBot.Api.Services;
using PaquitoBot.Application;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Infrastructure;
using PaquitoBot.Infrastructure.Integrations.Frappe;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found. Configure it with .NET User Secrets.");


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<IPasswordResetDelivery, PasswordResetDelivery>();
builder.Services.AddSingleton<PasswordResetQueue>();
builder.Services.AddSingleton<IPasswordResetQueue>(services => services.GetRequiredService<PasswordResetQueue>());
builder.Services.AddHostedService(services => services.GetRequiredService<PasswordResetQueue>());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("password-reset", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(10) }));
});
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddFrappeIntegration(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck<FrappeHealthCheck>(FrappeHealthCheck.Name);
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwaggerDocs();

app.UseHttpsRedirection();
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health/frappe", new HealthCheckOptions
{
    Predicate = registration => registration.Name == FrappeHealthCheck.Name
}).RequireAuthorization();
app.Run();
