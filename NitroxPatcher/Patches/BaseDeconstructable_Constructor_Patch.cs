using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Helper;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches
{
    /**
     * A DeconstructableBase is initialize when an base piece is fully created (unintuitively - this is the thing that tells the
     * build that this object can be deconstructed.) When this object is initialized, we store the game object in the transient
     * object manager so that later we can transfer the guid to it from the ghost.
     */
    public class BaseDeconstructable_Constructor_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly ConstructorInfo TARGET_METHOD = TARGET_CLASS.GetConstructor(new Type[] { });

        public static void Postfix(BaseDeconstructable __instance)
        {
            TransientLocalObjectManager.Add(TransientObjectType.LATEST_CONSTRUCTED_BASE_PIECE, __instance.gameObject);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
