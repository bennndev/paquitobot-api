using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaquitoBot.Infrastructure.Integrations.Frappe;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public sealed class FrappeOptionsTests
{
    [Fact]
    public void StartupAcceptsValidOptions()
    {
        using var services = BuildServices();

        services.GetRequiredService<IStartupValidator>().Validate();

        Assert.Equal(15, services.GetRequiredService<IOptions<FrappeOptions>>().Value.TimeoutSeconds);
    }

    [Theory]
    [InlineData("BaseUrl", null)]
    [InlineData("BaseUrl", "ftp://frappe.example")]
    [InlineData("BaseUrl", "https://test-key:test-secret@frappe.example")]
    [InlineData("BaseUrl", "https://frappe.example/?token=test-secret")]
    [InlineData("ApiKey", null)]
    [InlineData("ApiSecret", null)]
    [InlineData("ApiSecret", "test\r\nsecret")]
    [InlineData("TimeoutSeconds", "0")]
    public void StartupRejectsInvalidOptionsWithoutExposingValues(string key, string? value)
    {
        using var services = BuildServices(key, value);

        var error = Assert.Throws<OptionsValidationException>(() =>
            services.GetRequiredService<IStartupValidator>().Validate());

        Assert.Contains($"Frappe:{key}", error.Message);
        Assert.DoesNotContain("test-secret", error.Message);
        Assert.DoesNotContain("test-key", error.Message);
    }

    private static ServiceProvider BuildServices(string? overrideKey = null, string? overrideValue = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["Frappe:BaseUrl"] = "http://education.localhost:8000",
            ["Frappe:ApiKey"] = "test-key",
            ["Frappe:ApiSecret"] = "test-secret"
        };

        if (overrideKey is not null)
        {
            values[$"Frappe:{overrideKey}"] = overrideValue;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddFrappeIntegration(configuration);
        return services.BuildServiceProvider();
    }
}
