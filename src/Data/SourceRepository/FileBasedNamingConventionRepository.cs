using System.Text.Json;
using AzureNamingTool.Helpers;

namespace AzureNamingTool.Data.SourceRepository;

internal class FileBasedNamingConventionRepository : INamingConventionRepository
{
    private readonly FileSystemHelper _fileSystemHelper;

    public FileBasedNamingConventionRepository(
        FileSystemHelper fileSystemHelper)
    {
        _fileSystemHelper = fileSystemHelper;
    }

    public Task<string> ReadData(string fileName)
    {
        var data = _fileSystemHelper.ReadFile(fileName);
        return Task.FromResult(data);
    }

    public Task WriteData(string fileName, object data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var serializedData = JsonSerializer.Serialize(data, options);
        _fileSystemHelper.WriteFile(fileName, serializedData);
        return Task.CompletedTask;
    }
}