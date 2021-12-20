using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using NitroxLauncher.Install.Core;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxLauncher.Install;

public class InstallUrlProtocol : IInstaller, IUninstaller
{
    public bool IsInstalled
    {
        get
        {
            using RegistryKey commandKey = Registry.ClassesRoot.OpenSubKey(@"nitrox\shell\open\command");
            if (commandKey == null)
            {
                return false;
            }
            string command = commandKey.GetValue("") as string;
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }
            MatchCollection matches = Regex.Matches(command, @"""([^""]+)""");
            if (matches.Count < 2)
            {
                return false;
            }
            if (!File.Exists(matches[0].Groups[1].Value))
            {
                return false;
            }
            
            return true;
        }
    }

    public InstallResult Install()
    {
        InstallManager.Instance.SetUrlProtocolHandler("nitrox", NitroxUser.CurrentExecutablePath);
        return true;
    }

    public void Uninstall()
    {
        InstallManager.Instance.DeleteUrlProtocolHandler("nitrox");
    }
}
