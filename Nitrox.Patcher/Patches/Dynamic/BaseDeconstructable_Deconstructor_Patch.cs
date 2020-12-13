using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using static Nitrox.Client.GameLogic.Helper.TransientLocalObjectManager;

namespace Nitrox.Patcher.Patches.Dynamic
{
    /**
     * A DeconstructableBase is initialize when an base piece is fully created (unintuitively - this is the thing that tells the
     * build that this object can be deconstructed.)  When this oject is destroyed (we call deconstruct) we want to store its id
     * so that it can be later transfered to the ghost object that replaces it.
     */
    public class BaseDeconstructable_Deconstructor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(BaseDeconstructable __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            TransientLocalObjectManager.Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
