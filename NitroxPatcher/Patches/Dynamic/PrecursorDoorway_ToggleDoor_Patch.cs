using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class PrecursorDoorway_ToggleDoor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(PrecursorDoorway).GetMethod(nameof(PrecursorDoorway.ToggleDoor), BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(PrecursorDoorway __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            PrecursorDoorwayMetadata precursorDoorwayMetadata = new PrecursorDoorwayMetadata(__instance.isOpen);

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, precursorDoorwayMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
