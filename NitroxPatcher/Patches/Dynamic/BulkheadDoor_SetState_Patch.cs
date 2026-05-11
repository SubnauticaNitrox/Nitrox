using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes bulkhead door state changes across players.
/// Patches SetState instead of OnHandClick to catch both immediate and cinematic door opening modes.
/// The BulkheadDoor component is on a child object, so we look up the hierarchy for the NitroxEntity.
/// </summary>
public sealed partial class BulkheadDoor_SetState_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.SetState(default(bool)));

    public static void Prefix(BulkheadDoor __instance, bool open, out bool __state)
    {
        // Store the previous state to check if it actually changed
        __state = __instance.opened;
    }

    public static void Postfix(BulkheadDoor __instance, bool open, bool __state)
    {
        // Only broadcast if the state actually changed
        if (__state == open)
        {
            return;
        }

        if (__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            BulkheadDoorMetadata metadata = new(open);
            Resolve<Entities>().BroadcastMetadataUpdate(entity.Id, metadata);
        }
    }
}
