using System.Net;
using AffiliateMessages.Api;
using AffiliateMessages.Configuration;

namespace AffiliateMessages.Tests.Api;

public sealed class LocalUiHostTests
{
    [Fact]
    public async Task Host_ServesTheCompiledAngularApplicationFromTheSameOrigin()
    {
        await using var host = await LocalUiHost.StartAsync(new AppOptions());
        using var client = new HttpClient { BaseAddress = host.Address };

        using var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<app-root", html);
    }

    [Fact]
    public async Task Host_BindsOnlyToLoopbackOnAnOperatingSystemAssignedPort()
    {
        await using var host = await LocalUiHost.StartAsync(new AppOptions
        {
            OutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt"),
            LogFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log")
        });

        Assert.Equal(IPAddress.Loopback, IPAddress.Parse(host.Address.Host));
        Assert.InRange(host.Address.Port, 1, 65535);
        Assert.Equal(new[] { host.Address }, host.ListeningAddresses);
    }
}
