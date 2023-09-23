using System.Text.Json;

namespace AzureNamingTool.Helpers;

public class FileSystemHelper
{
    public string ReadFile(string fileName, string folderName = "settings/")
    {
        CheckFile(folderName + fileName);
        var data = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName));
        return data;
    }

    public void WriteFile(string fileName, string content, string folderName = "settings/")
    {
        CheckFile(folderName + fileName);
        var retries = 0;
        while (retries < 10)
        {
            try
            {
                using var fileStream = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName),
                    FileMode.Truncate, FileAccess.Write);
                StreamWriter sw = new(fileStream);
                sw.Write(content);
                sw.Flush();
                sw.Dispose();
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(50);
                retries++;
            }
        }
    }

    private void CheckFile(string fileName)
    {
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName))) 
            return;
        
        var file = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
        file.Close();

        for (var numTries = 0; numTries < 10; numTries++)
        {
            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), "[]");
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(50);
            }
        }
    }

    public void WriteConfiguration(object configData, string configFileName)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        WriteFile(configFileName, JsonSerializer.Serialize(configData, options));
    }

    public bool ResetConfiguration(string filename)
    {
        var result = false;

        // Get all the files in the repository folder
        DirectoryInfo dirRepository = new("repository");
        foreach (var file in dirRepository.GetFiles())
        {
            if (file.Name == filename)
            {
                // Copy the repository file to the settings folder
                file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name), true);
                result = true;
                // Clear any cached data

                break;
            }
        }

        return result;
    }

    public void MigrateDataToFile(string sourcefileName, string sourcefolderName, string destinationfilename,
        string destinationfolderName, bool delete)
    {
        // Get the source data
        var data = ReadFile(sourcefileName, sourcefolderName);

        // Write the destination data
        WriteFile(destinationfilename, data, destinationfolderName);

        // Check if the source file should be removed (In repository and settings folders)
        if (delete)
        {
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository/" + sourcefileName));
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + sourcefileName));
        }
    }
}