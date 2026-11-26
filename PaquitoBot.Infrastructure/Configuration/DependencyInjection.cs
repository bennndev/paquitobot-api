using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Infrastructure.Middleware;
using PaquitoBot.Infrastructure.Repositories.Academic;
using PaquitoBot.Infrastructure.Repositories.Chat;
using PaquitoBot.Infrastructure.Repositories.Finance;
using PaquitoBot.Infrastructure.Repositories.Sync;
using PaquitoBot.Infrastructure.Repositories.Users;
using PaquitoBot.Infrastructure.Services.Auth;
using PaquitoBot.Infrastructure.Services.Frappe.Sync;
using PaquitoBot.Infrastructure.Services.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Infrastructure.DbContext;
using PaquitoBot.Infrastructure.Repositories;
using PaquitoBot.Infrastructure.Services.Frappe;

namespace PaquitoBot.Infrastructure.Configuration;

public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection with .NET User Secrets.");

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret) && Encoding.UTF8.GetByteCount(settings.Secret) >= 32
                && !string.IsNullOrWhiteSpace(settings.Issuer) && !string.IsNullOrWhiteSpace(settings.Audience),
                "JWTVariable requires a secret of at least 32 bytes, an issuer and an audience.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecks().AddCheck<FrappeHealthCheck>(FrappeHealthCheck.Name);
        AddFrappeIntegration(services, configuration);

        services.AddDbContext<PaquitoBotDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IFrappeSyncedRepository<>), typeof(FrappeSyncedRepository<>));
        services.AddScoped<IUserPasswordHasher, UserPasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IContentHasher, ContentHasher>();
        services.AddMemoryCache();

        services.AddHttpClient<IFrappeOAuthService, FrappeOAuthService>().RemoveAllLoggers();
        services.AddHttpClient<IFrappeStudentSyncService, FrappeStudentSyncService>().RemoveAllLoggers();
        services.AddScoped<IKnowledgeRetriever, DocumentKnowledgeRetriever>();
        services.AddScoped<IAnswerProvider, SimulatedAnswerProvider>();

        services.AddScoped<IAssessmentPlanRepository, AssessmentPlanRepository>();
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<IAssessmentResultRepository, AssessmentResultRepository>();
        services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IProgramCourseRepository, ProgramCourseRepository>();
        services.AddScoped<IProgramEnrollmentRepository, ProgramEnrollmentRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IKnowledgeDocumentRepository, KnowledgeDocumentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IFinancialReadRepository, FinancialReadRepository>();
        services.AddScoped<IPaymentPostingRepository, PaymentPostingRepository>();
        services.AddScoped<ISyncRunRepository, SyncRunRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserStudentRepository, UserStudentRepository>();

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

        services.AddScoped<IFrappeSyncRunService, FrappeSyncRunService>();
        services.AddScoped<IEntitySynchronizer, AcademicPeriodSynchronizer>();
        services.AddScoped<IEntitySynchronizer, CourseScheduleSynchronizer>();
        services.AddScoped<IEntitySynchronizer, CourseSectionSynchronizer>();
        services.AddScoped<IEntitySynchronizer, CourseSynchronizer>();
        services.AddScoped<IEntitySynchronizer, InstructorSynchronizer>();
        services.AddScoped<IEntitySynchronizer, ProgramCourseSynchronizer>();
        services.AddScoped<IEntitySynchronizer, ProgramEnrollmentSynchronizer>();
        services.AddScoped<IEntitySynchronizer, ProgramSynchronizer>();
        services.AddScoped<IEntitySynchronizer, SectionEnrollmentSynchronizer>();
        services.AddScoped<IEntitySynchronizer, StudentSynchronizer>();

        services.AddScoped<IFrappeFeesPreviewService, FrappeFeesPreviewService>();
        services.AddScoped<IStudentFinancialReconciliationService, StudentFinancialReconciliationService>();
        services.AddScoped<IAcademicSyncService, AcademicSyncService>();
        services.AddScoped<IFrappeSnapshotPersister, FrappeStudentSnapshotPersister>();

        return services;
    }
}
