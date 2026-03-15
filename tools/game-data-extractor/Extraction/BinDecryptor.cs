using System.Security.Cryptography;

namespace OryxBot.GameDataExtractor.Extraction;

public static class BinDecryptor
{
    // The community standard key/IV for AES-256-CBC decryption of Albion Online bin files.
    // Placeholder 32-byte key and 16-byte IV for compilation purposes.
    private static readonly byte[] Key = new byte[32] { 
        0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 
        0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36,
        0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34,
        0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32 
    };
    
    private static readonly byte[] Iv = new byte[16] { 
        0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 
        0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36 
    };

    public static string DecryptBinFile(string binFilePath)
    {
        var data = File.ReadAllBytes(binFilePath);
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    public static async Task<string> DecryptBinFileAsync(string binFilePath, CancellationToken ct = default)
    {
        var data = await File.ReadAllBytesAsync(binFilePath, ct);
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return await sr.ReadToEndAsync(ct);
    }
}
