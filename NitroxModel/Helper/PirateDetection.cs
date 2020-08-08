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
            remove => pirateDetected -= value;
        }

        private static bool Trigger()
        {
            OnPirateDetected();
            return true;
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
            return File.Exists(steamDll) && new FileInfo(steamDll).Length > 209000;
        }

        private static void OnPirateDetected()
        {
            pirateDetected?.Invoke(null, EventArgs.Empty);
            HasTriggered = true;
        }
    }
}
