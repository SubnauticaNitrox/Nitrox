using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nitrox.Server.Subnautica.Core;

internal static class AssemblyResolver
{
    private static string currentExecutableDirectory;
    private static readonly Dictionary<string, Assembly> cache = [];
    private static readonly Dictionary<string, Assembly>.AlternateLookup<ReadOnlySpan<char>> cacheLookup = cache.GetAlternateLookup<ReadOnlySpan<char>>();
    private static string gameInstallPath;

    /// <summary>
    ///     The path to the game files so that game code can be loaded when needed.
    /// </summary>
    public static string GameInstallPath
    {
        get => Interlocked.Exchange(ref gameInstallPath, gameInstallPath);
        set => Interlocked.Exchange(ref gameInstallPath, value);
    }

    public static Assembly Handler(object sender, ResolveEventArgs args)
    {
        static Assembly ResolveFromLib(ReadOnlySpan<char> dllName)
        {
            string dllNameStr = GetAssemblyFileName(dllName).ToString();
            if (dllNameStr.EndsWith(".resources.dll"))
            {
                return null;
            }
            // If available, return cached assembly
            if (cache.TryGetValue(dllNameStr, out Assembly val))
            {
                return val;
            }

            // Load DLLs where this program (exe) is located
            string dllPath = Path.Combine(GetExecutableDirectory(), "lib", dllNameStr);
            if (GameInstallPath != null)
            {
                // Prefer to use Newtonsoft dll from game instead of our own due to protobuf issues. TODO: Remove when we do our own deserialization of game data instead of using the game's protobuf.
                if (dllPath.IndexOf("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase) >= 0 || !File.Exists(dllPath))
                {
                    // Try find game managed libraries
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        dllPath = Path.Combine(GameInstallPath, "Resources", "Data", "Managed", dllNameStr);
                    }
                    else
                    {
                        dllPath = Path.Combine(GameInstallPath, "Subnautica_Data", "Managed", dllNameStr);
                    }
                }
            }

            try
            {
                // Read assemblies as bytes as to not lock the file so that Nitrox can patch assemblies while server is running.
                cache[dllNameStr] = null; // Prevents potential infinite looping if Assembly.Load triggers this handler again.
                Assembly assembly = Assembly.Load(File.ReadAllBytes(dllPath));
                return cache[dllNameStr] = assembly;
            }
            catch
            {
                return null;
            }
        }

        Assembly assembly = ResolveFromLib(args.Name);
        if (assembly == null && !args.Name.Contains(".resources"))
        {
            ReadOnlySpan<char> assemblyFileName = GetAssemblyFileName(args.Name);
            if (cacheLookup.TryGetValue(assemblyFileName, out Assembly cachedAssembly) && cachedAssembly == null)
            {
                throw new FileNotFoundException($"Unable to resolve assembly {assemblyFileName}", $"{assemblyFileName}.dll");
            }
            assembly = Assembly.Load(args.Name);
        }

        return assembly;
    }

    private static ReadOnlySpan<char> GetAssemblyFileName(ReadOnlySpan<char> assemblyFullName)
    {
        assemblyFullName = assemblyFullName.Slice(0, Math.Max(assemblyFullName.IndexOf(','), 0));
        if (assemblyFullName.IsEmpty)
        {
            return null;
        }
        if (!assemblyFullName.EndsWith(".dll"))
        {
            assemblyFullName = string.Concat(assemblyFullName, ".dll");
        }
        return assemblyFullName;
    }

    private static string GetExecutableDirectory()
    {
        if (currentExecutableDirectory != null)
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
}
