namespace AzureNamingTool.Data.SourceRepository;

public interface INamingConventionRepository
{
    Task<string> ReadFile(string fileName);
    
    Task WriteFile(string fileName, string data);
}