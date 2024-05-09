using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Planter_AddItem_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter p) => p.AddItem(default, default));

    public static void Postfix(Plantable plantable, int slotID, Planter __instance)
    {
        Planter.PlantSlot slotByID = __instance.GetSlotByID(slotID);

        if (slotByID == null || slotByID.plantable != plantable)
        {
            return;
        }

        // When the planter accepts the new incoming seed, we want to send out metadata about what time the seed was planted.
        if (plantable.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(plantable, id);
        }
    }
}
