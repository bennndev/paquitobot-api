using PaquitoBot.Application.UseCases.Auth.Command;

namespace PaquitoBot.Api.Extensions;

public static class UseCaseRegistration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        var useCases = typeof(AuthenticateUseCase).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && (type.Name.EndsWith("UseCase") || type.Name.EndsWith("UseCaseCommand") || type.Name.EndsWith("UseCaseQuery")));

        foreach (var useCase in useCases)
        {
            services.AddScoped(useCase);
        }

        return services;
    }
}
