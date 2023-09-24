using System.Net.Http;
using System.Threading.Tasks;

namespace AzureNamingTool.UiTests.Contexts;

public class HomePageContext : ContextBase
{
    public HomePageContext(HttpClient httpClient) : base(httpClient)
    {
        SetDefaultSecurityRequestHeaders();
    }

    public async Task Given_request_home_page()
    {
        await Given_the_route_is_requested("home");
    }

    public async Task Given_request_route_that_doesnt_exist()
    {
        await Given_the_route_is_requested("i-do-not-exist");
    }
}