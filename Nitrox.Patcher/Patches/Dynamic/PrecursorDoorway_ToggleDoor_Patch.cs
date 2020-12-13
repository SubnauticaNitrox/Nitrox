using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
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
