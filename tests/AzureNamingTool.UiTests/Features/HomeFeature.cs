using System.Net;
using System.Threading.Tasks;
using AzureNamingTool.UiTests.Contexts;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;

namespace AzureNamingTool.UiTests.Features;

[FeatureDescription(
    @"As a Azure Naming Tool user
        I want to navigate to the home page
        So I can interact with the menu and use the system")]
public class HomeFeature : FeatureFixture
{
    [Scenario]
    public async Task ReturnsOkResult_ForDefaultHomePage()
    {
        await Runner
            .WithContext<HomePageContext>()
            .AddAsyncSteps(
                _ => _.Given_request_home_page(),
                _ => _.Then_response_code_should_be(HttpStatusCode.OK),
                _ => _.When_the_response_content_is_parsed(),
                _ => _.When_search_for_element_by_selector(ContextBase.PageContentSelector),
                _ => _.Then_the_found_element_should_contain("Instructions")
            ).RunAsync();
    }
}