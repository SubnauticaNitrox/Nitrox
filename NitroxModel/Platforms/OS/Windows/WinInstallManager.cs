using System;
using System.IO;
using Microsoft.Win32;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Platforms.OS.Windows;

public class WinInstallManager : InstallManager
{
    public override bool SetUrlProtocolHandler(string protocolName, string programPath)
    {
        programPath = Path.GetFullPath(programPath);
        if (!File.Exists(programPath) || !Path.GetExtension(programPath).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new FileNotFoundException("Program path does not point to an executable");
        }
        
        using RegistryKey key = Registry.ClassesRoot.OpenSubKey(protocolName);
        using RegistryKey protocolKey = Registry.ClassesRoot.CreateSubKey(protocolName);
        protocolKey.SetValue("", $"URL: {protocolName} Protocol");
        protocolKey.SetValue("URL Protocol", "");
        // Command should open program with the arguments given (represented by %1)
        using RegistryKey commandKey = protocolKey.CreateSubKey(@"shell\open\command");
        commandKey.SetValue("", $@"""{programPath}"" ""%1""");

        return true;
    }

    public override bool DeleteUrlProtocolHandler(string protocolName)
    {
        if (string.IsNullOrWhiteSpace(protocolName))
        {
            throw new ArgumentException("Protocol name must not be null or whitespace", nameof(protocolName));
        }
        
        return RegistryEx.Delete($@"Computer\HKEY_CLASSES_ROOT\{protocolName}");
    }
}
