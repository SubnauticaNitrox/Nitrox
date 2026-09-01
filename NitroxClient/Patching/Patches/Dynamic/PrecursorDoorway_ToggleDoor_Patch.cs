using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class PrecursorDoorway_ToggleDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorDoorway t) => t.ToggleDoor(default(bool)));

    public static void Postfix(PrecursorDoorway __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            PrecursorDoorwayMetadata precursorDoorwayMetadata = new(__instance.isOpen);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorDoorwayMetadata);
        }
    }
}
