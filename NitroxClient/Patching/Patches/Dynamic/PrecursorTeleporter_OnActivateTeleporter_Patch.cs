using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class PrecursorTeleporter_OnActivateTeleporter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorTeleporter t) => t.OnActivateTeleporter(default(string)));

    public static void Postfix(PrecursorTeleporter __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            PrecursorTeleporterMetadata precursorTeleporterMetadata = new(__instance.isOpen);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorTeleporterMetadata);
        }
    }
}
