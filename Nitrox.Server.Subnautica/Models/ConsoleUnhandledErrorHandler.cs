using System.IO;
using System.Linq;
using System.Reflection;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Server.Subnautica.Models;

static class ConsoleUnhandledErrorHandler
{
    public static void Attach()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Console.WriteLine(ex);
            LogAssemblyPathFromException(ex);
        }
        if (!Environment.UserInteractive || Console.IsInputRedirected || Console.In == StreamReader.Null)
        {
            return;
        }

        Console.WriteLine("Press L to open log folder before closing. Press any other key to close . . .");
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.L)
        {
            ProcessEx.OpenDirectory(Log.LogDirectory);
        }

        Environment.Exit(1);
    }

    /// <summary>
    ///     Logs the file path of assemblies referenced in type-related exceptions to help diagnose version conflicts.
    /// </summary>
    private static void LogAssemblyPathFromException(Exception ex)
    {
        while (true)
        {
            if (ex is AggregateException { InnerException: { } inner })
            {
                ex = inner;
            }
            else
            {
                break;
            }
        }
        string? assemblyName = ex switch
        {
            TypeLoadException t => t.GetType().GetField("_assemblyName", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(ex) as string,
            FileLoadException f => f.FileName,
            FileNotFoundException f => f.FileName,
            _ => null
        };
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return;
        }

        try
        {
            // Extract just the assembly name without version info
            Assembly? loadedAssembly = AppDomain
                                       .CurrentDomain
                                       .GetAssemblies()
                                       .OrderBy(a => a.GetName().FullName.Equals(assemblyName) ? 0 : 1) // Sorts full matches before weak matches.
                                       .FirstOrDefault(a => a.GetName().Name?.Equals(new AssemblyName(assemblyName).Name, StringComparison.OrdinalIgnoreCase) == true);

            if (loadedAssembly != null && loadedAssembly.GetName() is { } asmName)
            {
                Log.Error($"Error originates from '{asmName.Name}' v{asmName.Version}, file: {loadedAssembly.Location}");
            }
        }
        catch
        {
            // ignored
        }
    }
}
