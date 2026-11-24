using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Infrastructure.Integrations.Frappe;
using Xunit;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public sealed class FrappeClientTests
{
    [Fact]
    public async Task GetPageSendsTokenAndEncodesFieldsAndFilters()
    {
        using var handler = new StubHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("token", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-key:test-secret", request.Headers.Authorization?.Parameter);
            Assert.Equal("application/json", Assert.Single(request.Headers.Accept).MediaType);
            var uri = Assert.IsType<Uri>(request.RequestUri);
            Assert.Equal("/school/api/resource/Student%20Group", uri.AbsolutePath);
            var parameters = uri.Query[1..].Split('&')
                .Select(parameter => parameter.Split('=', 2))
                .ToDictionary(parts => parts[0], parts => Uri.UnescapeDataString(parts[1]));
            Assert.Equal("10", parameters["limit_start"]);
            Assert.Equal("2", parameters["limit_page_length"]);
            Assert.Equal("name asc", parameters["order_by"]);
            var fields = JsonSerializer.Deserialize<string[]>(parameters["fields"]);
            Assert.NotNull(fields);
            Assert.Equal(["name", "display_name"], fields);
            var filters = JsonSerializer.Deserialize<string[][]>(parameters["filters"]);
            Assert.NotNull(filters);
            Assert.Equal(["display_name", "like", "%A&B?%"], Assert.Single(filters));
            return Task.FromResult(Json("""{"data":[{"name":"R-001","display_name":"A&B?"}]}"""));
        });
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();
        var query = new FrappeQuery("Student Group", ["name", "display_name"],
            [new FrappeFilter("display_name", "like", "%A&B?%")], Offset: 10, PageSize: 2);

        var page = await client.GetPageAsync<Resource>(query, TestContext.Current.CancellationToken);

        Assert.Equal(new Resource("R-001", "A&B?"), Assert.Single(page.Items));
        Assert.Equal(11, page.NextOffset);
    }

    [Fact]
    public async Task GetPagePreservesRecordsWhenFrappeReturnsShortPages()
    {
        var responses = new Queue<(int Offset, string Body)>(
        [
            (0, """{"data":[{"name":"R-001"},{"name":"R-002"}]}"""),
            (2, """{"data":[{"name":"R-003"},{"name":"R-004"}]}"""),
            (4, """{"data":[{"name":"R-005"}]}"""),
            (5, """{"data":[]}""")
        ]);
        using var handler = new StubHandler((request, _) =>
        {
            var (offset, body) = responses.Dequeue();
            Assert.StartsWith($"?limit_start={offset}&", request.RequestUri?.Query);
            return Task.FromResult(Json(body));
        });
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();
        var query = new FrappeQuery("Student");
        var names = new List<string>();

        while (true)
        {
            var page = await client.GetPageAsync<Resource>(query, TestContext.Current.CancellationToken);
            names.AddRange(page.Items.Select(resource => resource.Name));
            if (page.NextOffset is not int offset)
            {
                break;
            }

            query = query with { Offset = offset };
        }

        Assert.Equal(["R-001", "R-002", "R-003", "R-004", "R-005"], names);
        Assert.Empty(responses);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, FrappeFailure.Authentication)]
    [InlineData(HttpStatusCode.Forbidden, FrappeFailure.Authentication)]
    [InlineData(HttpStatusCode.NotFound, FrappeFailure.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests, FrappeFailure.RateLimited)]
    [InlineData(HttpStatusCode.RequestTimeout, FrappeFailure.Timeout)]
    [InlineData(HttpStatusCode.GatewayTimeout, FrappeFailure.Timeout)]
    [InlineData(HttpStatusCode.InternalServerError, FrappeFailure.Unavailable)]
    [InlineData(HttpStatusCode.ServiceUnavailable, FrappeFailure.Unavailable)]
    [InlineData(HttpStatusCode.BadRequest, FrappeFailure.RequestRejected)]
    public async Task GetPageNormalizesHttpErrors(HttpStatusCode status, FrappeFailure failure)
    {
        using var handler = new StubHandler((_, _) => Task.FromResult(Json("sensitive-body", status)));
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();

        var error = await Assert.ThrowsAsync<FrappeException>(() =>
            client.GetPageAsync<Resource>(new("Student"), TestContext.Current.CancellationToken));

        Assert.Equal(failure, error.Failure);
        Assert.DoesNotContain("sensitive-body", error.Message);
        Assert.Null(error.InnerException);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"data":null}""")]
    [InlineData("""{"data":{}}""")]
    [InlineData("""{"data":[null]}""")]
    public async Task GetPageRejectsInvalidResponses(string body)
    {
        using var handler = new StubHandler((_, _) => Task.FromResult(Json(body)));
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();

        var error = await Assert.ThrowsAsync<FrappeException>(() =>
            client.GetPageAsync<Resource>(new("Student"), TestContext.Current.CancellationToken));

        Assert.Equal(FrappeFailure.InvalidResponse, error.Failure);
    }

    [Fact]
    public async Task GetPageNormalizesConnectionErrors()
    {
        using var handler = new StubHandler((_, _) => throw new HttpRequestException("private connection details"));
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();

        var error = await Assert.ThrowsAsync<FrappeException>(() =>
            client.GetPageAsync<Resource>(new("Student"), TestContext.Current.CancellationToken));

        Assert.Equal(FrappeFailure.Unavailable, error.Failure);
        Assert.Null(error.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetPageAppliesTimeoutToHeadersAndBody(bool delayBody)
    {
        using var handler = new StubHandler(async (_, token) =>
        {
            if (delayBody)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new DelayedStream()) };
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Expected the request to time out.");
        });
        await using var services = BuildServices(handler, timeoutSeconds: 1);
        var client = services.GetRequiredService<IFrappeClient>();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(5));

        var error = await Assert.ThrowsAsync<FrappeException>(() =>
            client.GetPageAsync<Resource>(new("Student"), deadline.Token));

        Assert.Equal(FrappeFailure.Timeout, error.Failure);
    }

    [Fact]
    public async Task GetPagePropagatesCallerCancellation()
    {
        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new StubHandler(async (_, token) =>
        {
            handlerStarted.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Expected the caller to cancel.");
        });
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var pending = client.GetPageAsync<Resource>(new("Student"), caller.Token);
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await caller.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Theory]
    [InlineData("""{"message":"service-account"}""", null)]
    [InlineData("""{"message":"Guest"}""", FrappeFailure.Authentication)]
    [InlineData("""{"message":null}""", FrappeFailure.InvalidResponse)]
    public async Task HealthRequiresAnAuthenticatedFrappeResponse(string body, FrappeFailure? failure)
    {
        using var handler = new StubHandler((request, _) =>
        {
            Assert.Equal("/school/api/method/frappe.auth.get_logged_user", request.RequestUri?.AbsolutePath);
            return Task.FromResult(Json(body));
        });
        await using var services = BuildServices(handler);
        var client = services.GetRequiredService<IFrappeClient>();
        var cancel = TestContext.Current.CancellationToken;

        if (failure is null)
        {
            await client.CheckHealthAsync(cancel);
            return;
        }

        var error = await Assert.ThrowsAsync<FrappeException>(() => client.CheckHealthAsync(cancel));
        Assert.Equal(failure, error.Failure);
    }

    private static ServiceProvider BuildServices(HttpMessageHandler handler, int timeoutSeconds = 15)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Frappe:BaseUrl"] = "https://frappe.example/school",
            ["Frappe:ApiKey"] = "test-key",
            ["Frappe:ApiSecret"] = "test-secret",
            ["Frappe:TimeoutSeconds"] = timeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)
        }).Build();
        var services = new ServiceCollection();
        services.AddFrappeIntegration(configuration).ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage Json(string content, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

    private sealed record Resource(string Name, string? DisplayName);

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => send(request, cancellationToken);
    }

    private sealed class DelayedStream : MemoryStream
    {
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }
    }
}
