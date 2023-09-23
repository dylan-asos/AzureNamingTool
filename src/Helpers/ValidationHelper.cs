using System.Text;
using System.Text.RegularExpressions;
using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool.Helpers;

public class ValidationHelper
{
    private readonly ResourceComponentService _resourceComponentService;
    private readonly GeneralHelper _generalHelper;

    public ValidationHelper(ResourceComponentService resourceComponentService, GeneralHelper generalHelper)
    {
        _resourceComponentService = resourceComponentService;
        _generalHelper = generalHelper;
    }

    public bool ValidatePassword(string text)
    {
        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasMinimum8Chars = new Regex(@".{8,}");

        var isValidated = hasNumber.IsMatch(text) && hasUpperChar.IsMatch(text) && hasMinimum8Chars.IsMatch(text);

        return isValidated;
    }

    public bool ValidateShortName(string type, string value, string? parentcomponent = null)
    {
        var valid = false;

        ResourceComponent resourceComponent = new();

        var serviceResponse =
            _resourceComponentService.GetItems(true);

        if (serviceResponse.Success)
        {
            if (serviceResponse.ResponseObject != null)
            {
                var resourceComponents = (List<ResourceComponent>) serviceResponse.ResponseObject!;

                if (resourceComponents != null)
                {
                    // Check if it's a custom component
                    if (type == "CustomComponent")
                    {
                        if (parentcomponent!= null)
                        {
                            resourceComponent = resourceComponents.Find(x =>
                                _generalHelper.NormalizeName(x.Name, true) ==
                                _generalHelper.NormalizeName(parentcomponent, true))!;
                        }
                    }
                    else
                    {
                        resourceComponent = resourceComponents.Find(x => x.Name == type)!;
                    }

                    if (resourceComponent!= null)
                    {
                        // Check if the name mathces the length requirements for the component
                        if (value.Length >= Convert.ToInt32(resourceComponent.MinLength) &&
                            value.Length <= Convert.ToInt32(resourceComponent.MaxLength))
                        {
                            valid = true;
                        }
                    }
                }
            }
        }

        return valid;
    }

    public ValidateNameResponse ValidateGeneratedName(ResourceType resourceType, string name, string delimiter)
    {
        ValidateNameResponse response = new();

        var valid = true;
        StringBuilder sbMessage = new();

        // Check regex
        // Validate the name against the resource type regex
        Regex regx = new(resourceType.Regx);
        var match = regx.Match(name);
        var delimiterValid = !string.IsNullOrEmpty(delimiter);

        // Check to see if the delimiter has been set
        if (!match.Success)
        {
            if (delimiterValid)
            {
                // Strip the delimiter in case that is causing the issue
                name = name.Replace(delimiter, "");

                var match2 = regx.Match(name);
                if (!match2.Success)
                {
                    sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                    sbMessage.Append(Environment.NewLine);
                    valid = false;
                }
                else
                {
                    sbMessage.Append(
                        "The specified delimiter is not allowed for this resource type and has been removed.");
                    sbMessage.Append(Environment.NewLine);
                }
            }
            else
            {
                sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                sbMessage.Append(Environment.NewLine);
                valid = false;
            }
        }


        // Check min length
        if (int.TryParse(resourceType.LengthMin, out _))
        {
            if (name.Length < int.Parse(resourceType.LengthMin))
            {
                sbMessage.Append("Generated name is less than the minimum length for the selected resource type.");
                sbMessage.Append(Environment.NewLine);
                valid = false;
            }
        }

        // Check max length
        if (int.TryParse(resourceType.LengthMax, out _))
        {
            if (name.Length > int.Parse(resourceType.LengthMax))
            {
                // Strip the delimiter in case that is causing the issue
                name = name.Replace(delimiter, "");
                if (name.Length > int.Parse(resourceType.LengthMax))
                {
                    sbMessage.Append("Generated name is more than the maximum length for the selected resource type.");
                    sbMessage.Append(Environment.NewLine);
                    sbMessage.Append(
                        "Please remove any optional components or contact your admin to update the required components for this resource type.");
                    sbMessage.Append(Environment.NewLine);
                    valid = false;
                }
                else
                {
                    sbMessage.Append(
                        "Generated name with the selected delimiter is more than the maximum length for the selected resource type. The delimiter has been removed.");
                    sbMessage.Append(Environment.NewLine);
                }
            }
        }

        // Check invalid characters
        if (!string.IsNullOrEmpty(resourceType.InvalidCharacters))
        {
            // Loop through each character
            foreach (var c in resourceType.InvalidCharacters)
            {
                // Check if the name contains the character
                if (name.Contains(c))
                {
                    sbMessage.Append("Name cannot contain the following character: " + c);
                    sbMessage.Append(Environment.NewLine);
                    valid = false;
                }
            }
        }

        // Check start character
        if (!string.IsNullOrEmpty(resourceType.InvalidCharactersStart))
        {
            // Loop through each character
            foreach (var c in resourceType.InvalidCharactersStart)
            {
                // Check if the name contains the character
                if (name.StartsWith(c))
                {
                    sbMessage.Append("Name cannot start with the following character: " + c);
                    sbMessage.Append(Environment.NewLine);
                    valid = false;
                }
            }
        }

        // Check start character
        if (!string.IsNullOrEmpty(resourceType.InvalidCharactersEnd))
        {
            // Loop through each character
            foreach (var c in resourceType.InvalidCharactersEnd)
            {
                // Check if the name contains the character
                if (name.EndsWith(c))
                {
                    sbMessage.Append("Name cannot end with the following character: " + c);
                    sbMessage.Append(Environment.NewLine);
                    valid = false;
                }
            }
        }

        // Check consecutive character
        if (!string.IsNullOrEmpty(resourceType.InvalidCharactersConsecutive))
        {
            // Loop through each character
            foreach (var c in resourceType.InvalidCharactersConsecutive)
            {
                // Check if the name contains the character
                var current = name[0];
                for (var i = 1; i < name.Length; i++)
                {
                    var next = name[i];
                    if (current == next && current == c)
                    {
                        sbMessage.Append("Name cannot contain the following consecutive character: " + next);
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                        break;
                    }

                    current = next;
                }
            }
        }

        response.Valid = valid;
        response.Name = name;
        response.Message = sbMessage.ToString();

        return response;
    }

    public static bool CheckNumeric(string value)
    {
        Regex regx = new("^[0-9]+$");
        var match = regx.Match(value);
        return match.Success;
    }

    public static bool CheckAlphanumeric(string value)
    {
        Regex regx = new("^[a-zA-Z0-9]+$");
        var match = regx.Match(value);
        return match.Success;
    }
}