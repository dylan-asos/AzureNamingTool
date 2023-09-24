using System.Security.Cryptography;
using System.Text;

namespace AzureNamingTool.Helpers;

public class EncryptionHelper
{
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
}