using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.Utils;

public static class Hashing
{
    public static async Task<byte[]> GetCachedSha256ByFilePath(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheFile = GetSha256CacheFilePath(filePath);
            return await File.ReadAllBytesAsync(cacheFile, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            return [];
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return [];
        }
    }

    public static async Task<byte[]> GetAndStoreSha256ForFile(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheFile = GetSha256CacheFilePath(filePath);
            byte[] sha256 = await GetSha256(filePath, cancellationToken);
            await File.WriteAllBytesAsync(cacheFile, sha256, cancellationToken);
            return sha256;
        }
        catch (FileNotFoundException)
        {
            return [];
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return [];
        }
    }

    public static async Task<byte[]> GetSha256(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return SHA256.HashData(await File.ReadAllBytesAsync(filePath, cancellationToken));
        }
        catch (FileNotFoundException)
        {
            return [];
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            return [];
        }
    }

    private static string GetSha256CacheFilePath(string targetFilePath)
    {
        string filePathMd5 = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(targetFilePath)));
        return Path.Combine(NitroxUser.CachePath, Path.ChangeExtension(filePathMd5, "sha256"));
    }
}
