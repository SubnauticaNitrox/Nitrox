using Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.ReactiveUI;

namespace Nitrox.Launcher;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

        LoadAvalonia(args);
    }

    private static void LoadAvalonia(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder builder = AppBuilder.Configure<App>()
                                                .UsePlatformDetect()
                                                .LogToTrace()
                                                .UseReactiveUI();

        // The Wayland renderer on Linux using GPU rendering is not (yet) supported by Avalonia.
        // Waiting on PR: https://github.com/AvaloniaUI/Avalonia/pull/11546 to enable rendering on GPU.
        if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
        {
            builder = builder.With(new X11PlatformOptions
            {
                RenderingMode = new[] { X11RenderingMode.Software }
            });
        }

        return builder;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        static Assembly ResolveFromLib(string dllName)
        {
            string dllFileName = dllName.Split(',')[0];
            if (!dllFileName.EndsWith(".dll"))
            {
                dllFileName += ".dll";
            }

            string dllPath = Path.Combine(Environment.CurrentDirectory, "lib", dllFileName);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dllFileName);
            }

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Nitrox dll missing: {dllPath}");
            }
            return Assembly.LoadFile(dllPath);
        }

        return ResolveFromLib(args.Name) ?? Assembly.Load(args.Name);
    }
}
