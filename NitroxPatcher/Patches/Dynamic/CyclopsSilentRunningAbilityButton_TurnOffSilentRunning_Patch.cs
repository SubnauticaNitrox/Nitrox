using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsSilentRunningAbilityButton_TurnOffSilentRunning_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSilentRunningAbilityButton t) => t.TurnOffSilentRunning());

        public static void Postfix(CyclopsSilentRunningAbilityButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);

            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeSilentRunning(id, false);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
