using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.Deconstruct());

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (!__result) return;
            if (__instance.constructedAmount <= 0f)
            {
                if (!__instance.TryGetComponent(out NitroxEntity nitroxEntity))
                {
                    return;
                }

                Resolve<Building>().DeconstructionComplete(__instance.gameObject);
                Log.Debug("Finished deconstructing, now removing ghost NitroxEntity");

                GeometryRespawnManager.NitroxIdsToIgnore.Add(nitroxEntity.Id);
                Log.Debug($"Added ghost to ignore list: {nitroxEntity.Id}");
            }
            else
            {
                Resolve<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
