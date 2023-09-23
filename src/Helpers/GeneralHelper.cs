using System.Security.Cryptography;
using System.Text;

namespace AzureNamingTool.Helpers;

public class GeneralHelper
{
    public object? GetPropertyValue(object sourceData, string propName)
    {
        return sourceData!.GetType()!.GetProperty(propName)!.GetValue(sourceData, null);
    }

    public string EncryptString(string text, string keyString)
    {
        var iv = new byte[16];
        byte[] array;
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = Encoding.UTF8.GetBytes(keyString);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(text);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public string DecryptString(string cipherText, string keyString)
    {
        var iv = new byte[16];
        var buffer = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public bool IsBase64Encoded(string value)
    {
        var base64encoded = false;
        try
        {
            var byteArray = Convert.FromBase64String(value);
            base64encoded = true;
        }
        catch (FormatException)
        {
            // The string is not base 64. Dismiss the error and return false
        }

        return base64encoded;
    }

    public string NormalizeName(string name, bool lowercase)
    {
        var newname = name.Replace("Resource", "").Replace(" ", "");
        if (lowercase)
        {
            newname = newname.ToLower();
        }

        return newname;
    }

    public string SetTextEnabledClass(bool enabled)
    {
        return enabled ? "" : "disabled-text";
    }

    public string[] FormatResoureType(string type)
    {
        var returntype = new string[3];
        returntype[0] = type;
        // Make sure it is a full resource type name
        if (type.Contains("("))
        {
            returntype[0] = type.Substring(0, type.IndexOf("(")).Trim();
        }

        if (type!= null && returntype[0]!= null)
        {
            // trim any details out of the value
            if (returntype[0].Contains(" -"))
            {
                returntype[1] = returntype[0].Substring(0, returntype[0].IndexOf(" -")).Trim();
            }

            // trim any details out of the value
            if (type.Contains("(") && type.Contains(")"))
            {
                {
                    var intstart = type.IndexOf("(") + 1;
                    returntype[2] = string.Concat(type.Substring(intstart).TakeWhile(x => x != ')'));
                }
            }
        }

        return returntype;
    }
}