using System;
using System.Threading;
using NitroxModel.Platforms.OS.Windows;

namespace NitroxModel.Platforms.OS.Shared
{
    public abstract class InstallManager
    {
        private static readonly Lazy<InstallManager> instance = new(() => Environment.OSVersion.Platform switch
                                                                    {
                                                                        PlatformID.Unix => throw new NotSupportedException(),
                                                                        PlatformID.MacOSX => throw new NotSupportedException(),
                                                                        _ => new WinInstallManager()
                                                                    },
                                                                    LazyThreadSafetyMode.ExecutionAndPublication);

        public static InstallManager Instance => instance.Value;
        
        public abstract bool SetUrlProtocolHandler(string protocolName, string programPath);
        public abstract bool DeleteUrlProtocolHandler(string protocolName);
    }
}
