using System.Net;
using System.Threading.Tasks;
using AzureNamingTool.UiTests.Contexts;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace AzureNamingTool.UiTests.Features;

[FeatureDescription(
    @"As a Azure Naming Tool user
        I want to navigate to the reference page
        So I can view how the naming tool works")]
public class ReferenceFeature : FeatureFixture
{
    [Scenario]
    public async Task ReturnsOkResult_ForReferencePage()
    {
        await Runner
            .WithContext<ReferencePageContext>()
            .AddAsyncSteps(
                _ => _.Given_request_reference_page(),
                _ => _.Then_response_code_should_be(HttpStatusCode.OK),
                _ => _.When_the_response_content_is_parsed(),
                _ => _.When_search_for_element_by_id("jumpto"),
                _ => _.Then_the_found_element_should_contain("Use the filters below to jump to a specific resource type.")
            ).RunAsync();
    }
}