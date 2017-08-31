using System.Reflection;
using Harmony;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches.Client
{
    public abstract class CinematicModeTrigger_Generic_Patch<T> : IHandTarget_OnHandClick_Generic_Patch<T>
        where T : CinematicModeTriggerBase
    {
        public static readonly MethodInfo TARGET_METHOD_OnPlayerCinematicModeEnd = TARGET_CLASS.GetMethod("OnPlayerCinematicModeEnd", BindingFlags.Public | BindingFlags.Instance);

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD_OnPlayerCinematicModeEnd, "OnPlayerCinematicModeEnd_Prefix");
            base.Patch(harmony);
        }

        protected abstract void OnPlayerCinematicModeEnd(T instance, string guid);

        public static bool OnPlayerCinematicModeEnd_Prefix(T __instance, PlayerCinematicController cinematicController)
        {
            Log.Debug($"{Time.realtimeSinceStartup} {nameof(T)}.OnPlayerCinematicModeEnd: {cinematicController}");
            CinematicModeTrigger_Generic_Patch<T> self = (CinematicModeTrigger_Generic_Patch<T>)Self;
            Optional<string> opGuid = self.GetGuid(__instance);
            if (opGuid.IsPresent())
            {
                self.OnPlayerCinematicModeEnd(__instance, opGuid.Get());
            }
            return true;
        }
    }
}
