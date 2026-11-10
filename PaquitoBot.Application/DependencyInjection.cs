using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Common.Behaviors;

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

        return services;
    }
}
