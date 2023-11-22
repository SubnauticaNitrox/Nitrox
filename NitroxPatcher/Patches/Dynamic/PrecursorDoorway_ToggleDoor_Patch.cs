using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PrecursorDoorway_ToggleDoor_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((PrecursorDoorway t) => t.ToggleDoor(default(bool)));

    public static void Postfix(PrecursorDoorway __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            PrecursorDoorwayMetadata precursorDoorwayMetadata = new(__instance.isOpen);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorDoorwayMetadata);
        }
    }
}
