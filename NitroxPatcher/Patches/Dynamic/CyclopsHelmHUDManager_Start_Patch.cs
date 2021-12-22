using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsHelmHUDManager_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Start());

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            GameObject cyclopsObject = __instance.transform.parent.gameObject;
            CyclopsModel cyclops = Resolve<Vehicles>().GetVehicles<CyclopsModel>(NitroxEntity.GetId(cyclopsObject));
            __instance.hudActive = true;
            __instance.engineToggleAnimator.SetTrigger(cyclops.EngineState ? "EngineOn" : "EngineOff");
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
