using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsEngineChangeState_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsEngineChangeState t) => t.OnClick());

        public static void Postfix(CyclopsEngineChangeState __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            Resolve<Cyclops>().BroadcastToggleEngineState(id, __instance.motorMode.engineOn, __instance.startEngine);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
