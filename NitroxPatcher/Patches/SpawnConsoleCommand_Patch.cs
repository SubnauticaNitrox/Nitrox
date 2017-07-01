using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxPatcher.Patches
{
    [HarmonyPatch(typeof(SpawnConsoleCommand))]
    [HarmonyPatch("Awake")]
    public class SpawnConsoleCommand_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(SpawnConsoleCommand __instance)
        {
            __instance.gameObject.AddComponent<Multiplayer>();
            return true;
        }
    }
}
