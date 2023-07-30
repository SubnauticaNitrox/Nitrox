using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Planter_IsAllowedToAdd_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter p) => p.IsAllowedToAdd(default(Pickupable), default(bool)));

    public static void Postfix(Pickupable pickupable, bool __result)
    {
        // When the planter accepts the new incoming seed, we want to send out metadata about what time the seed was planted.
        if (__result && pickupable.TryGetIdOrWarn(out NitroxId id))
        {
            Plantable plantable = pickupable.GetComponent<Plantable>();
            Resolve<Entities>().EntityMetadataChanged(plantable, id);
        }
    }
}
