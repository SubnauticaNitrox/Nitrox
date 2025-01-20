using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Avalonia.Platform;

namespace Nitrox.Launcher.Models.Utils;

public static class AssetHelper
{
    private static readonly string assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new Exception("Unable to get Assembly name");
    private static readonly Dictionary<string, Uri> assetPathCache = [];

    public static Uri GetFullAssetPath(string assetPath)
    {
        if (assetPathCache.TryGetValue(assetPath, out Uri fullPath))
        {
            return fullPath;
        }

        Uri uri = assetPath.StartsWith("avares://") ? new Uri(assetPath) : new Uri($"avares://{assemblyName}{assetPath}");
        if (!AssetLoader.Exists(uri) && !Avalonia.Controls.Design.IsDesignMode)
        {
            return assetPathCache[assetPath] = default;
        }
        return assetPathCache[assetPath] = uri;
    }

    public static T GetAssetFromStream<T>(string assetPath, Func<Stream, T> streamToDataFactory) => AssetLoader<T>.GetFromStream(assetPath, streamToDataFactory);

    private static class AssetLoader<T>
    {
        private static readonly Dictionary<string, T> assetCache = [];
        private static readonly Lock assetCacheLock = new();

        public static T GetFromStream(string rawUri, Func<Stream, T> streamToDataFactory)
        {
            T data;
            lock (assetCacheLock)
            {
                if (assetCache.TryGetValue(rawUri, out data))
                {
                    return data;
                }
            }
            // In design mode, resource aren't yet embedded.
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                using Stream stream = File.OpenRead(TryGetPathFromLocalFileSystem(rawUri));
                data = streamToDataFactory(stream);
            }
            if (data == null)
            {
                using Stream stream = AssetLoader.Open(GetFullAssetPath(rawUri));
                data = streamToDataFactory(stream);
            }
            lock (assetCacheLock)
            {
                assetCache.Add(rawUri, data);
            }
            return data;
        }

        private static string TryGetPathFromLocalFileSystem(string fileUri)
        {
            string targetedProject = Path.GetDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault(part => !part.Contains("Designer", StringComparison.Ordinal) && part.EndsWith("dll", StringComparison.OrdinalIgnoreCase) && File.Exists(part)));
            while (targetedProject != null && !Directory.EnumerateFileSystemEntries(targetedProject, "*.csproj", SearchOption.TopDirectoryOnly).Any())
            {
                targetedProject = Path.GetDirectoryName(targetedProject);
            }
            if (targetedProject == null)
            {
                return null;
            }
            ReadOnlySpan<char> fileUriSpan = fileUri.AsSpan();
            while (fileUriSpan.StartsWith("/") || fileUriSpan.StartsWith("\\"))
            {
                fileUriSpan = fileUriSpan[1..];
            }
            return Path.Combine(targetedProject, fileUriSpan.ToString());
        }
    }
}
