using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaquitoBot.Api.Configuration;
using PaquitoBot.Application.Configuration;
using PaquitoBot.Infrastructure.Configuration;
using PaquitoBot.Infrastructure.Services.Frappe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwaggerDocs();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/frappe", new HealthCheckOptions
{
    Predicate = registration => registration.Name == FrappeHealthCheck.Name
}).RequireAuthorization();
app.Run();
