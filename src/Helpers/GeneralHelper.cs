namespace AzureNamingTool.Helpers;

public class GeneralHelper
{
    public object? GetPropertyValue(object sourceData, string propName)
    {
        return sourceData!.GetType()!.GetProperty(propName)!.GetValue(sourceData, null);
    }
    
    public bool IsBase64Encoded(string value)
    {
        var base64Encoded = false;
        try
        {
            var byteArray = Convert.FromBase64String(value);
            base64Encoded = true;
        }
        catch (FormatException)
        {
            // The string is not base 64. Dismiss the error and return false
        }

        return base64Encoded;
    }

    public string NormalizeName(string name, bool lowercase)
    {
        var newName = name.Replace("Resource", "").Replace(" ", "");
        if (lowercase)
        {
            newName = newName.ToLower();
        }

        return newName;
    }

    public string SetTextEnabledClass(bool enabled)
    {
        return enabled ? "" : "disabled-text";
    }

    public string[] FormatResourceType(string type)
    {
        var returnType = new string[3];
        returnType[0] = type;
        
        // Make sure it is a full resource type name
        if (type.Contains('('))
        {
            returnType[0] = type.Substring(0, type.IndexOf("(", StringComparison.Ordinal)).Trim();
        }

        if (type != null && returnType[0] != null)
        {
            // trim any details out of the value
            if (returnType[0].Contains(" -"))
            {
                returnType[1] = returnType[0].Substring(0, returnType[0].IndexOf(" -", StringComparison.Ordinal)).Trim();
            }

            // trim any details out of the values
            if (type.Contains('(') && type.Contains(')'))
            {
                var startPosition = type.IndexOf("(", StringComparison.Ordinal) + 1;
                returnType[2] = string.Concat(type.Substring(startPosition).TakeWhile(x => x != ')'));
            }
        }

        return returnType;
    }
}