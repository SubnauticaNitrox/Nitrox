using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    /**
     * A DeconstructableBase is initialize when an base piece is fully created (unintuitively - this is the thing that tells the
     * build that this object can be deconstructed.)  When this oject is destroyed (we call deconstruct) we want to store its id
     * so that it can be later transfered to the ghost object that replaces it.
     */
    public class BaseDeconstructable_Deconstructor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseDeconstructable t) => t.Deconstruct());

        public static void Prefix(BaseDeconstructable __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, id);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
