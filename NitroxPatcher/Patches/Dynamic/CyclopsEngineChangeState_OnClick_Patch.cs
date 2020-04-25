using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsEngineChangeState_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsEngineChangeState).GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsEngineChangeState __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastToggleEngineState(id, __instance.motorMode.engineOn, (bool)__instance.ReflectionGet("startEngine"));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
