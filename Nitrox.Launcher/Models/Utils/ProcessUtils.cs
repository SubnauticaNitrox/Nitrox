using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    /// <summary>
    ///     Starts the current app as a new instance.
    /// </summary>
    public static void StartSelf(params string[] arguments)
    {
        string executableFilePath = NitroxUser.ExecutableFilePath ?? Environment.ProcessPath;
        // On Linux, entry assembly is .dll file but real executable is without extension.
        string temp = Path.ChangeExtension(executableFilePath, null);
        if (File.Exists(temp))
        {
            executableFilePath = temp;
        }
        temp = Path.ChangeExtension(executableFilePath, ".exe");
        if (File.Exists(temp))
        {
            executableFilePath = temp;
        }
        
        if (arguments.Contains("--allow-instances"))
        {
            arguments = [..arguments, "--allow-instances"];
        }
        using Process proc = StartProcessDetached(new ProcessStartInfo(executableFilePath!, arguments));
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
