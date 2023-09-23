using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models;

public class CustomComponent
{
    private string _shortName = string.Empty;
    public long Id { get; set; }

    [Required] public string ParentComponent { get; set; } = string.Empty;

    [Required] public string Name { get; set; } = string.Empty;

    [Required]
    public string ShortName
    {
        get => _shortName;
        set => _shortName = value?.ToLower()!;
    }

    public int SortOrder { get; set; } = 0;
    public string MinLength { get; set; } = "1";
    public string MaxLength { get; set; } = "10";
}