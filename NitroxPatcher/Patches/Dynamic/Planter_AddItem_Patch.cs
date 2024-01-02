using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Planter_AddItem_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter p) => p.AddItem(default));

    public static void Prefix(InventoryItem item)
    {
        Pickupable pickupable = item.item;
        // When the planter accepts the new incoming seed, we want to send out metadata about what time the seed was planted.
        if (pickupable && pickupable.TryGetIdOrWarn(out NitroxId id))
        {
            Plantable plantable = pickupable.GetComponent<Plantable>();
            Resolve<Entities>().EntityMetadataChanged(plantable, id);
        }
    }
}
