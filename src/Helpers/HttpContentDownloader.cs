namespace AzureNamingTool.Helpers;

public class HttpContentDownloader
{
    public async Task<string> DownloadString(string url)
    {
        HttpClient httpClient = new();
        var data = await httpClient.GetStringAsync(url);

        return data;
    }
}