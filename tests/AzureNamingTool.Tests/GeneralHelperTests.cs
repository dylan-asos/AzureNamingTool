using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using FluentAssertions;

namespace AzureNamingTool.Tests;

public class GeneralHelperTests
{
    [Fact]
    public void NormalizeName_RemovesResourceFromName_WhenInputContains()
    {
        const string input = "ResourceOrg";

        var generalHelper = new GeneralHelper();
        var result = generalHelper.NormalizeName(input, true);

        result.Should().Be("org");
    }
    
    [Fact]
    public void FormatResourceType_SplitsInpuIntoArray_WhenInputContains()
    {
        const string input = "Web/sites - Static Web App (stapp)";

        var generalHelper = new GeneralHelper();
        var result = generalHelper.FormatResourceType(input);

        result.Length.Should().Be(3);
        result[0].Should().Be("Web/sites - Static Web App");
        result[1].Should().Be("Web/sites");
        result[2].Should().Be("stapp");
    }
}