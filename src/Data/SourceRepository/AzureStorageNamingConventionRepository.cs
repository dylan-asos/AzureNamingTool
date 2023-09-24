namespace AzureNamingTool.Data.SourceRepository;

internal class AzureStorageNamingConventionRepository : INamingConventionRepository
{
    public async Task<string> ReadFile(string fileName)
    {
        return await Task.FromResult("AzureStorageNamingConventionRepository");
    }

    public Task WriteFile(string fileName, string data)
    {
        return Task.CompletedTask;
    }
}