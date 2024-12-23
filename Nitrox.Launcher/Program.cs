using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Svg.Skia;
using Nitrox.Launcher.Models.Utils;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace Nitrox.Launcher;

internal static class Program
{
    // Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code before AppMain is called
    // Things aren't initialized yet and stuff might break
    [STAThread]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

        LoadAvalonia(args);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LoadAvalonia(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            HandleUnhandledException(ex);
        }
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        // https://github.com/wieslawsoltes/Svg.Skia?tab=readme-ov-file#avalonia-previewer
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);

        return App.Create();
    }

    internal static void HandleUnhandledException(Exception ex)
    {
        if (App.IsCrashReport)
        {
            Log.Error(ex, "Error while trying to show crash report");
            return;
        }

        Log.Error(ex, "!!!Nitrox Launcher Crash!!!");

        // Write crash report if we're not reporting one right now.
        try
        {
            string executableFilePath = NitroxUser.ExecutableFilePath ?? Environment.ProcessPath;
            string executableRoot = Path.GetDirectoryName(executableFilePath);
            if (executableFilePath != null && executableRoot != null)
            {
                string crashReportFile = Path.Combine(executableRoot, App.CRASH_REPORT_FILE_NAME);
                File.WriteAllText(crashReportFile, ex.ToString());
                ProcessUtils.StartSelf("--crash-report");
            }
            else
            {
                Log.Error(ex, "Unable to get executable file path for writing crash report.");
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            Console.WriteLine(exception);
        }
        Environment.Exit(1);
    }

    private static class AssemblyResolver
    {
        private static string currentExecutableDirectory;

        public static Assembly Handler(object sender, ResolveEventArgs args)
        {
            static Assembly ResolveFromLib(ReadOnlySpan<char> dllName)
            {
                dllName = dllName.Slice(0, dllName.IndexOf(','));
                if (!dllName.EndsWith(".dll"))
                {
                    dllName = string.Concat(dllName, ".dll");
                }

                if (dllName.EndsWith(".resources.dll"))
                {
                    return null;
                }

                string dllNameStr = dllName.ToString();

                string dllPath = Path.Combine(GetExecutableDirectory(), "lib", dllNameStr);
                if (!File.Exists(dllPath))
                {
                    dllPath = Path.Combine(GetExecutableDirectory(), dllNameStr);
                }

                try
                {
                    return Assembly.LoadFile(dllPath);
                }
                catch
                {
                    return null;
                }
            }

            Assembly assembly = ResolveFromLib(args.Name);
            if (assembly == null && !args.Name.Contains(".resources"))
            {
                assembly = Assembly.Load(args.Name);
            }

            return assembly;
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
}
