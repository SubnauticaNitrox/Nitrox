using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

// The IncubatorActivationTerminal class is attached to the ion cube pillar near the incubator.  This is what the player clicks
// power up the main incubator terminal window.
public sealed partial class IncubatorActivationTerminal_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IncubatorActivationTerminal t) => t.OnHandClick(default(GUIHand)));

    public static void Prefix(IncubatorActivationTerminal __instance)
    {
        // The server only knows about the main incubator platform which is the direct parent
        GameObject platform = __instance.transform.parent.gameObject;

        if (!__instance.incubator.powered && Inventory.main.container.Contains(TechType.PrecursorIonCrystal) &&
            platform.TryGetIdOrWarn(out NitroxId id))
        {
            IncubatorMetadata metadata = new(true, false);
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata);
        }
    }
}
