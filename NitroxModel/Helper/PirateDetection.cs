using System;
using System.IO;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Helper
{
    public static class PirateDetection
    {
        public static bool HasTriggered { get; private set; }

        /// <summary>
        ///     Event that calls subscribers if the pirate detection triggered successfully.
        ///     New subscribers are immediately invoked if the pirate flag has been set at the time of subscription.
        /// </summary>
        public static event EventHandler PirateDetected
        {
            add
            {
                pirateDetected += value;

                // Invoke new subscriber immediately if pirate has already been detected.
                if (HasTriggered)
                {
                    value?.Invoke(null, EventArgs.Empty);
                }
            }
            remove => pirateDetected -= value;
        }

        public static bool TriggerOnDirectory(string subnauticaRoot)
        {
            if (!IsPirateByDirectory(subnauticaRoot))
            {
                return false;
            }

            OnPirateDetected();
            return false;
        }

        private static event EventHandler pirateDetected;

        private static bool IsPirateByDirectory(string subnauticaRoot)
        {
            string subdirDll = Path.Combine(subnauticaRoot, "Subnautica_Data", "Plugins", "x86_64", "steam_api64.dll");
            if (File.Exists(subdirDll) && !FileSystem.Instance.IsTrustedFile(subdirDll))
            {
                return true;
            }
            // Dlls might be in root if cracked game (to override DLLs in sub directories).
            string rootDll = Path.Combine(subnauticaRoot, "steam_api64.dll");
            if (File.Exists(rootDll) && !FileSystem.Instance.IsTrustedFile(rootDll))
            {
                return true;
            }

            return false;
        }

        private static void OnPirateDetected()
        {
            pirateDetected?.Invoke(null, EventArgs.Empty);
            HasTriggered = true;
        }
    }
}
