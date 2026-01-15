using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nitrox.Server.Subnautica;

internal static class AssemblyResolver
{
    private static string currentExecutableDirectory = string.Empty;
    private static readonly Dictionary<string, AssemblyCacheEntry> resolvedAssemblyCache = [];
    private static string? gamePath;

    /// <summary>
    ///     The path to the game files so that game code can be loaded when needed.
    /// </summary>
    public static string? GamePath
    {
        get => Interlocked.Exchange(ref gamePath, gamePath);
        set => Interlocked.Exchange(ref gamePath, value);
    }

    public static Assembly? Handler(object sender, ResolveEventArgs args)
    {
        Assembly? assembly = ResolveFromLib(args.Name);
        if (assembly == null && !args.Name.Contains(".resources"))
        {
            assembly = Assembly.Load(args.Name);
        }

        return assembly;
    }

    private static Assembly? ResolveFromLib(ReadOnlySpan<char> dllName)
    {
        dllName = dllName.Slice(0, Math.Max(dllName.IndexOf(','), 0));
        if (dllName.IsEmpty)
        {
            return null;
        }
        if (!dllName.EndsWith(".dll"))
        {
            dllName = string.Concat(dllName, ".dll");
        }
        if (dllName.EndsWith(".resources.dll"))
        {
            return null;
        }
        string dllNameStr = dllName.ToString();
        // If available, return cached assembly
        if (resolvedAssemblyCache.TryGetValue(dllNameStr, out AssemblyCacheEntry cacheEntry) && cacheEntry is { Assembly: { } cachedAssembly })
        {
            return cachedAssembly;
        }
        if (cacheEntry == null)
        {
            cacheEntry = new AssemblyCacheEntry(0, null);
            resolvedAssemblyCache[dllNameStr] = cacheEntry;
        }

        // Load DLLs where this program (exe) is located
        string dllPath = Path.Combine(GetExecutableDirectory(), "lib", dllNameStr);
        // Prefer to use Newtonsoft dll from game instead of our own due to protobuf issues. TODO: Remove when we do our own deserialization of game data instead of using the game's protobuf.
        if (dllPath.Contains("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase) || !File.Exists(dllPath))
        {
            if (TryGetDllFromGameInstall(dllNameStr) is { } validPath)
            {
                dllPath = validPath;
            }
        }

        try
        {
            // Read assemblies as bytes as to not lock the file so that Nitrox can patch assemblies while server is running.
            cacheEntry.Assembly = Assembly.Load(File.ReadAllBytes(dllPath));
            return cacheEntry.Assembly;
        }
        catch
        {
            cacheEntry.Attempts++;
            if (cacheEntry.Attempts >= 5)
            {
                throw new FileNotFoundException($"Failed to load DLL '{dllName}' at: {dllPath}");
            }
            return null;
        }
    }

    /// <remarks>
    ///     Nitrox.Model access wrapped as a method to prevent stackoverflow during DLL resolving.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string? TryGetDllFromGameInstall(string dllName)
    {
        if (string.IsNullOrWhiteSpace(GamePath))
        {
            return null;
        }
        // Try find game managed libraries
        return Path.Combine(NitroxUser.GamePath, GameInfo.Subnautica.DataFolder, "Managed", dllName);
    }

    private static string GetExecutableDirectory()
    {
        if (!string.IsNullOrEmpty(currentExecutableDirectory))
        {
            return currentExecutableDirectory;
        }
        string pathAttempt = Assembly.GetEntryAssembly()?.Location;
        if (string.IsNullOrWhiteSpace(pathAttempt))
        {
            using Process proc = Process.GetCurrentProcess();
            pathAttempt = proc.MainModule?.FileName;
        }
        return currentExecutableDirectory = new Uri(Path.GetDirectoryName(pathAttempt ?? ".") ?? Directory.GetCurrentDirectory()).LocalPath;
    }

    private record AssemblyCacheEntry(int Attempts, Assembly? Assembly)
    {
        public int Attempts { get; set; } = Attempts;
        public Assembly? Assembly { get; set; } = Assembly;
    }
}
