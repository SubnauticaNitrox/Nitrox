using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BulkheadDoor_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = typeof(BulkheadDoor).GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(BulkheadDoor __instance)
        {
            bool open = __instance.isOpen || __instance.isOpening;
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            BulkheadDoorMetadata bulkheadDoorMetadata = new BulkheadDoorMetadata(open);

            Log.InGame($"Door is now {(open ? "Open" : "Close")}");

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, bulkheadDoorMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
