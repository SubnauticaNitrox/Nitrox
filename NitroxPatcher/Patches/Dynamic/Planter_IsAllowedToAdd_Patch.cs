using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class Planter_IsAllowedToAdd_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter p) => p.IsAllowedToAdd(default(Pickupable), default(bool)));

    public static void Postfix(Pickupable pickupable, bool __result)
    {
        // When the planter accepts the new incoming seed, we want to send out metadata about what time the seed was planted. 
        if (__result)
        {
            Plantable plantable = pickupable.GetComponent<Plantable>();
            NitroxId id = NitroxEntity.GetId(plantable.gameObject);

            Resolve<Entities>().EntityMetadataChanged(plantable, id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
