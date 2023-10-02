using System.Net.Http;
using System.Threading.Tasks;

namespace AzureNamingTool.UiTests.Contexts;

public class ReferencePageContext : ContextBase
{
    public ReferencePageContext(HttpClient httpClient) : base(httpClient)
    {
        SetDefaultSecurityRequestHeaders();
    }

    public async Task Given_request_reference_page()
    {
        await Given_the_route_is_requested("reference");
    }
    
}