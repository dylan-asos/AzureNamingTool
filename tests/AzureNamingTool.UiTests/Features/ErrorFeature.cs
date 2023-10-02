using System.Net;
using System.Threading.Tasks;
using AzureNamingTool.UiTests.Contexts;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;

namespace AzureNamingTool.UiTests.Features;

[FeatureDescription(
    @"As an azure naming tool engineer
        I want errors to be handled gracefully
        So users understand when something has gone wrong in the system")]
public class ErrorFeature : FeatureFixture
{
    [Scenario]
    public async Task ReturnsNotFoundView_ForUnknownRoute()
    {
        await Runner
            .WithContext<HomePageContext>()
            .AddAsyncSteps(
                _ => _.Given_request_route_that_doesnt_exist(),
                _ => _.Then_response_code_should_be(HttpStatusCode.OK),
                _ => _.When_the_response_content_is_parsed(),
                _ => _.Then_the_document_should_contain(
                    "Whoa, it looks like that page went and r-u-n-n-o-f-t! Try again!")
            ).RunAsync();
    }
}