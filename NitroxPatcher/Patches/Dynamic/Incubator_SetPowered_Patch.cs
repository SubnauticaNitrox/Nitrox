using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class Incubator_SetPowered_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Incubator).GetMethod(nameof(Incubator.SetPowered), BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(Incubator __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            IncubatorMetadata incubatorMetadata = new IncubatorMetadata(__instance.powered, __instance.hatched);

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, incubatorMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
