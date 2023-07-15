using System.Reflection;
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
    public sealed partial class BaseDeconstructable_Deconstructor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseDeconstructable t) => t.Deconstruct());

        public static void Prefix(BaseDeconstructable __instance)
        {
            if (__instance.TryGetIdOrWarn(out NitroxId id))
            {
                Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, id);
            }
        }
    }
}
