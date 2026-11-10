using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;
using PaquitoBot.Infrastructure.Repositories;

namespace PaquitoBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaquitoBotDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    
        var repositories = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Repository"));

        foreach (var implementation in repositories)
        {
            var contract = implementation.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementation.Name}");

            if (contract is not null)
            {
                services.AddScoped(contract, implementation);
            }
        }

        return services;
    }
}
