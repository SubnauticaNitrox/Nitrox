using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GhostCrafter_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GhostCrafter t) => t.OnCraftingBegin(default(TechType), default(float)));

        public static void Postfix(GhostCrafter __instance, TechType techType, float duration)
        {
            NitroxId crafterId = NitroxEntity.GetId(__instance.gameObject);
            Resolve<Entities>().BroadcastMetadataUpdate(crafterId, new CrafterMetadata(techType.ToDto(), DayNightCycle.main.timePassedAsFloat, duration));

            // Async request to be the person to auto-pickup the result. In the future this can be improved to lock down all crafting based on ownership
            // but will require redoing our hooks. 
            Resolve<SimulationOwnership>().RequestSimulationLock(crafterId, SimulationLockType.EXCLUSIVE);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
