namespace AzureNamingTool.Data.SourceRepository;

internal class AzureStorageNamingConventionRepository : INamingConventionRepository
{
    public async Task<string> ReadData(string fileName)
    {
        return await Task.FromResult("AzureStorageNamingConventionRepository");
    }

    public Task WriteData(string fileName, object data)
    {
        return Task.CompletedTask;
    }
}