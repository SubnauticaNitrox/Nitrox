using Avalonia;
using System;
using Avalonia.ReactiveUI;

namespace Nitrox.Launcher;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

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
}
