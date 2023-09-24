namespace AzureNamingTool.Data.SourceRepository;

public interface INamingConventionRepository
{
    Task<string> ReadData(string fileName);
    
    Task WriteData(string fileName, object data);
}