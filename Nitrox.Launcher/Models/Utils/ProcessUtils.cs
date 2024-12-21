using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DynamicData;
using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Utils;

public static class ProcessUtils
{
    public static Process StartProcessDetached(ProcessStartInfo startInfo)
    {
        if (!string.IsNullOrWhiteSpace(startInfo.Arguments))
        {
            throw new NotSupportedException($"Arguments must be supplied via {startInfo.ArgumentList}");
        }

        // On Linux, processes are started as child by default. So we wrap as shell command to start detached from current process.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            List<string> newArgs = ["-c", string.Join(" ", ["nohup", $"'{startInfo.FileName}'", string.Join(" ", startInfo.ArgumentList), ">/dev/null 2>&1", "&"])];
            startInfo.FileName = "/bin/sh";
            startInfo.ArgumentList.Clear();
            startInfo.ArgumentList.AddRange(newArgs);
        }

        return Process.Start(startInfo);
    }

    public static void RestartApp()
    {
        string executableFilePath = NitroxUser.ExecutableFilePath ?? Environment.ProcessPath;
        string noExtension = Path.ChangeExtension(executableFilePath, null);
        if (File.Exists(noExtension))
        {
            executableFilePath = noExtension;
        }
        using Process proc = StartProcessDetached(new ProcessStartInfo(executableFilePath!, ["--allow-instances"]));
        Environment.Exit(0);
    }

    /// <summary>
    ///     Opens the Url in the default browser. Forces the Uri scheme as Https.
    /// </summary>
    public static void OpenUrl(string url)
    {
        UriBuilder urlBuilder = new(url) { Scheme = Uri.UriSchemeHttps, Port = -1 };
        using Process proc = Process.Start(new ProcessStartInfo
        {
            FileName = urlBuilder.Uri.ToString(),
            UseShellExecute = true,
            Verb = "open"
        });
    }
}
