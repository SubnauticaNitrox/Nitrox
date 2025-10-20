using System.Diagnostics;
using System.IO;

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
        }
        if (!Environment.UserInteractive || Console.IsInputRedirected || Console.In == StreamReader.Null)
        {
            return;
        }

        Console.WriteLine("Press L to open log folder before closing. Press any other key to close . . .");
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.L)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Log.LogDirectory,
                Verb = "open",
                UseShellExecute = true
            })?.Dispose();
        }

        Environment.Exit(1);
    }
}
