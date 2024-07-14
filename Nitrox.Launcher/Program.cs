﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using NitroxModel.Logger;

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
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

        LoadAvalonia(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        Log.Setup();

        AppBuilder builder = AppBuilder.Configure<App>()
                                       .UsePlatformDetect()
                                       .LogToTrace()
                                       .UseReactiveUI();

        // The Wayland renderer on Linux using GPU rendering is not (yet) supported by Avalonia.
        // Waiting on PR: https://github.com/AvaloniaUI/Avalonia/pull/11546 to enable rendering on GPU.
        if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
        {
            builder = builder.With(new X11PlatformOptions { RenderingMode = [X11RenderingMode.Software] });
        }

        return builder;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LoadAvalonia(string[] args) =>
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

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

            return ResolveFromLib(args.Name) ?? Assembly.Load(args.Name);
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
