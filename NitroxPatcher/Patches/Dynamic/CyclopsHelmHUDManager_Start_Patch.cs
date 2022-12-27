using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsHelmHUDManager_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Start());

        public static void Postfix(CyclopsHelmHUDManager __instance)
        {
            NitroxEntity entity = __instance.GetComponent<NitroxEntity>();

            // This patch is invoked by an additive cyclops scene at the beginning.  This just loads the cylops prefabs and stuff but
            // doesn't represent an actual construction event. Only sync with the cyclops model when we have a tagged nitrox entity.
            if (entity)
            {
                CyclopsModel cyclops = Resolve<Vehicles>().GetVehicles<CyclopsModel>(entity.Id);
                __instance.hudActive = true;
                __instance.engineToggleAnimator.SetTrigger(cyclops.EngineState ? "EngineOn" : "EngineOff");
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
