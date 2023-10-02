using AzureNamingTool.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureNamingTool.UiTests.Client;

public class AzureNamingToolApplication : WebApplicationFactory<Program>
{
    private string HostUrl { get; } = "https://localhost:5005";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<StateContainer>(provider => new StateContainer
            {
                Verified = true,
                Password = true,
                Admin = true
            });
        });

        builder.UseUrls(HostUrl);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("ui-tests");
        return base.CreateHost(builder);
    }
}