using System;
using System.IO;

namespace NitroxModel.Helper
{
    public static class PirateDetection
    {
        private static bool HasTriggered { get; set; }

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
            remove { pirateDetected -= value; }
        }

        public static bool Trigger()
        {
            OnPirateDetected();
            return false;
        }

        public static bool TriggerOnDirectory(string subnauticaRoot)
        {
            return IsPirateByDirectory(subnauticaRoot) && Trigger();
        }

        private static event EventHandler pirateDetected;

        private static bool IsPirateByDirectory(string subnauticaRoot)
        {
            string steamDll = Path.Combine(subnauticaRoot, "steam_api64.dll");

            // Check for a modified steam dll
            if (File.Exists(steamDll))
            {
                if (new FileInfo(steamDll).Length > 209000)
                {
                    return true;
                }
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
