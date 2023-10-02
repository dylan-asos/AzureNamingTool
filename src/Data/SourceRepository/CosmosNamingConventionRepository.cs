namespace AzureNamingTool.Data.SourceRepository;

internal class CosmosNamingConventionRepository : INamingConventionRepository
{
    public async Task<string> ReadData(string fileName)
    {
        
        
        return await Task.FromResult("CosmosNamingConventionRepository");
    }

    public Task WriteData(string fileName, object data)
    {
        return Task.CompletedTask;
    }
}