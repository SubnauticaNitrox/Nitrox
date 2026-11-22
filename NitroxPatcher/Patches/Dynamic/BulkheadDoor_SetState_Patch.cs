using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes bulkhead door state changes across players.
/// Patches SetState instead of OnHandClick to catch both immediate and cinematic door opening modes.
/// Wreck doors are spawned from server entities and carry their own NitroxEntity (on a parent object).
/// Doors built into a base don't: they are handled by <see cref="BulkheadDoorManager"/>.
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

        // Check if the door is a child of a base
        if (__instance.TryGetComponentInParent(out Base @base, true))
        {
            // The game only subscribes to onStateChange once a base door is fully set up, so a null one means the door
            // is being (re)built by the base geometry and its state is restored from the base's faces, not toggled by a player
            if (__instance.onStateChange == null)
            {
                return;
            }
            @base.gameObject.EnsureComponent<BulkheadDoorManager>().OnDoorStateChanged(__instance, open);
            return;
        }

        // Check if the door is a Nitrox entity (wreck doors)
        if (__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            BulkheadDoorMetadata metadata = new(open);
            Resolve<Entities>().BroadcastMetadataUpdate(entity.Id, metadata);
        }
    }
}
