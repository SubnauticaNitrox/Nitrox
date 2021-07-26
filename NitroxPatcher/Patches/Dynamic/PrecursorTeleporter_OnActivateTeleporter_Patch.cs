using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class PrecursorTeleporter_OnActivateTeleporter_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(PrecursorTeleporter).GetMethod("OnActivateTeleporter", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(PrecursorTeleporter __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            PrecursorTeleporterMetadata precursorTeleporterMetadata = new PrecursorTeleporterMetadata(__instance.isOpen);

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, precursorTeleporterMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
