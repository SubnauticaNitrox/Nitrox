using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class PrecursorDoorway_ToggleDoor_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDoorway t) => t.ToggleDoor(default(bool)));

        public static void Postfix(PrecursorDoorway __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            PrecursorDoorwayMetadata precursorDoorwayMetadata = new(__instance.isOpen);

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, precursorDoorwayMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
