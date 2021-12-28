using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsHelmHUDManager_StopPiloting_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.StopPiloting());

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            __instance.hudActive = true;
            Resolve<Cyclops>().BroadcastChangeSonarState(id, false);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
