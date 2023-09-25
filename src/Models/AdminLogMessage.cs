namespace AzureNamingTool.Models;

public class AdminLogMessage
{
    public long? Id { get; set; } = 0;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = "System";

    public LogLevel Level { get; set; } = LogLevel.Information;
}