using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
