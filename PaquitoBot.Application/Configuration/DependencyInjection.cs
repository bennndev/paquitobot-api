using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Behaviors;

namespace PaquitoBot.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en-US");

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton(TimeProvider.System);


        return services;
    }
}
