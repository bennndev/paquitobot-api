using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Common.Behaviors;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en-US");

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IFrappeSyncRunService, FrappeSyncRunService>();

        services.AddSingleton(TimeProvider.System);

        var synchronizers = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IEntitySynchronizer).IsAssignableFrom(type));

        foreach (var synchronizer in synchronizers)
        {
            services.AddScoped(typeof(IEntitySynchronizer), synchronizer);
        }

        return services;
    }
}
