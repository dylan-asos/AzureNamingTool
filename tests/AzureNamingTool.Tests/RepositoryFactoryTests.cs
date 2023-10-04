using Azure.Storage.Blobs;
using AzureNamingTool.Data.SourceRepository;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using FluentAssertions;

namespace AzureNamingTool.Tests;

public class RepositoryFactoryTests
{
    [Fact]
    public void GetRepository_ReturnsAzureStorageRepository_WhenSiteConfigurationIsAzureStorage()
    {
        const string input = "AzureStorage";

        var factory = new RepositoryFactory(new SiteConfiguration {RepositoryType = input},
            new FileSystemHelper(),
            new BlobServiceClient("UseDevelopmentStorage=true"));

        var result = factory.GetRepository();

        result.Should().BeOfType<AzureStorageNamingConventionRepository>();
    }

    [Fact]
    public void GetRepository_ReturnsFileBasedRepository_WhenSiteConfigurationIsFileOrDefault()
    {
        const string input = "File";

        var factory = new RepositoryFactory(new SiteConfiguration {RepositoryType = input},
            new FileSystemHelper(),
            new BlobServiceClient("UseDevelopmentStorage=true"));

        var result = factory.GetRepository();

        result.Should().BeOfType<FileBasedNamingConventionRepository>();
    }
}