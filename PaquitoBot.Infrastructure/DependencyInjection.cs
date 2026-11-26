using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Infrastructure.Data;
using PaquitoBot.Infrastructure.Repositories;
using PaquitoBot.Infrastructure.Services;
using PaquitoBot.Infrastructure.Services.Frappe;

namespace PaquitoBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaquitoBotDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IFrappeSyncedRepository<>), typeof(FrappeSyncedRepository<>));
        services.AddScoped<IUserPasswordHasher, UserPasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddMemoryCache();
        services.AddSingleton(new HttpClient());
        services.AddScoped<IFrappeOAuthService, FrappeOAuthService>();
        services.AddScoped<IFrappeStudentSyncService, FrappeStudentSyncService>();
        services.AddScoped<IKnowledgeRetriever, DocumentKnowledgeRetriever>();
        services.AddScoped<IAnswerProvider, SimulatedAnswerProvider>();
        services.AddScoped<IReportExcelExporter, ReportExcelExporter>();

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

        services.AddScoped<IFrappeSource<FrappeStudentDto>, FrappeStudentSource>();
        services.AddScoped<IFrappeSource<FrappeAcademicPeriodDto>, FrappeAcademicPeriodSource>();
        services.AddScoped<IFrappeSource<FrappeProgramDto>, FrappeProgramSource>();
        services.AddScoped<IFrappeSource<FrappeCourseDto>, FrappeCourseSource>();
        services.AddScoped<IFrappeSource<FrappeInstructorDto>, FrappeInstructorSource>();
        services.AddScoped<IFrappeSource<FrappeProgramCourseDto>, FrappeProgramCourseSource>();
        services.AddScoped<IFrappeSource<FrappeCourseSectionDto>, FrappeCourseSectionSource>();
        services.AddScoped<IFrappeSource<FrappeCourseScheduleDto>, FrappeCourseScheduleSource>();
        services.AddScoped<IFrappeSource<FrappeProgramEnrollmentDto>, FrappeProgramEnrollmentSource>();
        services.AddScoped<IFrappeSource<FrappeSectionEnrollmentDto>, FrappeSectionEnrollmentSource>();

        return services;
    }
}
