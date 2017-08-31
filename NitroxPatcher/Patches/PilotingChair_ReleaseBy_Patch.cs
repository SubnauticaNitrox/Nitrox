using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    class PilotingChair_ReleaseBy_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PilotingChair);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ReleaseBy", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(PilotingChair __instance, Player player)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            Log.Debug("PilotingChair ReleaseBy called");
            // Because __instance.transform.parent.gameObject doesn't work.
            simulationOwnership.ReleaseOwnership(GuidHelper.GetGuid(__instance.GetComponentInParent<SubRoot>().gameObject));
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
