using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace Nitrox.Launcher
{
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
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToTrace()
                         .UseReactiveUI()
                         .With(new X11PlatformOptions
                         {
                             // The Wayland renderer on Linux using GPU rendering is not supported by Avalonia.
                             // Waiting on PR: https://github.com/AvaloniaUI/Avalonia/pull/8352 to enable rendering on GPU.
                             UseGpu = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is null
                         });
    }
}
