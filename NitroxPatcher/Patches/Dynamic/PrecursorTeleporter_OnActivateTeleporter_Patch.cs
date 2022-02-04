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
    class PrecursorTeleporter_OnActivateTeleporter_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorTeleporter t) => t.OnActivateTeleporter(default(string)));

        public static void Postfix(PrecursorTeleporter __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            PrecursorTeleporterMetadata precursorTeleporterMetadata = new(__instance.isOpen);

            Entities entities = NitroxServiceLocator.LocateService<Entities>();
            entities.BroadcastMetadataUpdate(id, precursorTeleporterMetadata);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
