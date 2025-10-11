using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StarshipDoor_OnDoorToggle_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StarshipDoor t) => t.OnDoorToggle());

    public static void Postfix(StarshipDoor __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            StarshipDoorMetadata starshipDoorMetadata = new(__instance.doorLocked, __instance.doorOpen);
            Resolve<Entities>().BroadcastMetadataUpdate(id, starshipDoorMetadata);
        }
    }
}
