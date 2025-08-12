using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StarshipDoor_LockDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StarshipDoor t) => t.LockDoor());

    public static void Prefix(StarshipDoor __instance)
    {
        if (!__instance.doorLocked && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            StarshipDoorMetadata starshipDoorMetadata = new(__instance.doorLocked, __instance.doorOpen);
            Resolve<Entities>().BroadcastMetadataUpdate(id, starshipDoorMetadata);
        }
    }
}
