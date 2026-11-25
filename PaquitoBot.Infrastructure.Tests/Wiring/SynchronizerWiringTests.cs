using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Infrastructure.Integrations.Frappe;

namespace PaquitoBot.Infrastructure.Tests.Wiring;

public class SynchronizerWiringTests
{
    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frappe:BaseUrl"] = "http://frappe.test:8000",
                ["Frappe:ApiKey"] = "test-key",
                ["Frappe:ApiSecret"] = "test-secret"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure("Host=localhost;Database=unused;Username=unused;Password=unused");
        services.AddFrappeIntegration(configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = false });
    }

    [Fact]
    public void AllTenEntitySynchronizers_AreRegisteredAndResolvable()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var names = scope.ServiceProvider.GetServices<IEntitySynchronizer>().Select(s => s.EntityName).ToList();

        Assert.Equal(10, names.Count);
        Assert.Equal(
            ["AcademicPeriod", "Course", "CourseSchedule", "CourseSection", "Instructor", "Program", "ProgramCourse", "ProgramEnrollment", "SectionEnrollment", "Student"],
            names.Order().ToList());
    }

    [Fact]
    public void Synchronizers_RespectDependencyOrder()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var ordered = scope.ServiceProvider.GetServices<IEntitySynchronizer>().OrderBy(s => s.Order).ToDictionary(s => s.EntityName, s => s.Order);

        Assert.True(ordered["Course"] < ordered["ProgramCourse"]);
        Assert.True(ordered["Program"] < ordered["ProgramCourse"]);
        Assert.True(ordered["ProgramCourse"] < ordered["CourseSection"]);
        Assert.True(ordered["CourseSection"] < ordered["CourseSchedule"]);
        Assert.True(ordered["Instructor"] < ordered["CourseSchedule"]);
        Assert.True(ordered["Student"] < ordered["ProgramEnrollment"]);
        Assert.True(ordered["Student"] < ordered["SectionEnrollment"]);
        Assert.True(ordered["CourseSection"] < ordered["SectionEnrollment"]);
        Assert.True(ordered["AcademicPeriod"] < ordered["CourseSection"]);
    }
}
