using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Nitrox.Launcher;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

        LoadAvalonia(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder builder = AppBuilder.Configure<App>()
                                       .UsePlatformDetect()
                                       .LogToTrace()
                                       .UseReactiveUI();

        // The Wayland renderer on Linux using GPU rendering is not (yet) supported by Avalonia.
        // Waiting on PR: https://github.com/AvaloniaUI/Avalonia/pull/11546 to enable rendering on GPU.
        if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
        {
            builder = builder.With(new X11PlatformOptions { RenderingMode = [ X11RenderingMode.Software ] });
        }

        return builder;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LoadAvalonia(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        static Assembly ResolveFromLib(ReadOnlySpan<char> dllName)
        {
            dllName = dllName.Slice(0, dllName.IndexOf(','));
            if (!dllName.EndsWith(".dll"))
            {
                dllName = string.Concat(dllName, ".dll");
            }
            string dllNameStr = dllName.ToString();

            string dllPath = Path.Combine(Environment.CurrentDirectory, "lib", dllNameStr);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", dllNameStr);
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

        return ResolveFromLib(args.Name) ?? Assembly.Load(args.Name);
    }
}
