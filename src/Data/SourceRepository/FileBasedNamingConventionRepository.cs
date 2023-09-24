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

    public Task<string> ReadFile(string fileName)
    {
        var data = _fileSystemHelper.ReadFile(fileName);
        return Task.FromResult(data);
    }

    public Task WriteFile(string fileName, string data)
    {
        _fileSystemHelper.WriteFile(fileName, data);
        return Task.CompletedTask;
    }
}