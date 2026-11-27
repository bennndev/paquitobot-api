
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
app.MapControllers();
app.MapHealthChecks("/health/frappe", new HealthCheckOptions
{
    Predicate = registration => registration.Name == FrappeHealthCheck.Name
}).RequireAuthorization();
app.Run();
