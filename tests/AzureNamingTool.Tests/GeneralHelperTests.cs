using FluentAssertions;

namespace AzureNamingTool.Tests;

public class GeneralHelperTests
{
    [Fact]
    public void NormalizeName_RemovesResourceFromName_WhenInputContains()
    {
        const string input = "ResourceOrg";

        var generalHelper = new Helpers.GeneralHelper();
        var result = generalHelper.NormalizeName(input, true);

        result.Should().BeSameAs("org");
    }
}