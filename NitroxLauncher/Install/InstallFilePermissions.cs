using System;
using System.IO;
using NitroxLauncher.Install.Core;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxLauncher.Install;

public class InstallFilePermissions : IInstaller
{
    private readonly Func<string> targetPathProvider;

    public bool IsInstalled => FileSystem.Instance.IsWritable(Directory.GetCurrentDirectory()) && FileSystem.Instance.IsWritable(targetPathProvider());

    public InstallFilePermissions(Func<string> targetPathProvider)
    {
        this.targetPathProvider = targetPathProvider;
    }

    public InstallResult Install()
    {
        if (!FileSystem.Instance.SetFullAccessToCurrentUser(Directory.GetCurrentDirectory()) || !FileSystem.Instance.SetFullAccessToCurrentUser(targetPathProvider()))
        {
            return InstallResult.AuthErrorResult;
        }

        return true;
    }
}
