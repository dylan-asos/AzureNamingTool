namespace AzureNamingTool.Data.SourceRepository;

internal class CosmosNamingConventionRepository : INamingConventionRepository
{
    public async Task<string> ReadFile(string fileName)
    {
        return await Task.FromResult("CosmosNamingConventionRepository");
    }

    public Task WriteFile(string fileName, string data)
    {
        return Task.CompletedTask;
    }
}