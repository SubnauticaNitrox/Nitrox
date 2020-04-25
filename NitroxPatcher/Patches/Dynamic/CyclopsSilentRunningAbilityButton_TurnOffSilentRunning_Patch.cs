using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsSilentRunningAbilityButton_TurnOffSilentRunning_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsSilentRunningAbilityButton).GetMethod("TurnOffSilentRunning", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsSilentRunningAbilityButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);

            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeSilentRunning(id, false);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
