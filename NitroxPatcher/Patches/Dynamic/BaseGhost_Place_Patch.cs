using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseGhost_Place_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Place", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(BaseGhost __instance)
        {
            // Null out fields that will be used for storing state
            TransientLocalObjectManager.Remove(TransientObjectType.LATEST_BASE_WITH_NEW_CONSTRUCTION);
            TransientLocalObjectManager.Remove(TransientObjectType.LATEST_BASE_CELL_WITH_NEW_CONSTRUCTION);

            // In this case, our piece is part of an existing, larger base.  We'll record the base and offset.  
            // We don't directly record the piece as it will be re-built when the base rebuilds geometry (this
            // happens each time a piece is built, subnautica destroys all others and replaces them).
            if (__instance.TargetBase != null)
            {
                Log.Debug("Placed BaseGhost is a new piece of an existing base");
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_BASE_WITH_NEW_CONSTRUCTION, __instance.TargetBase);
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_BASE_CELL_WITH_NEW_CONSTRUCTION, __instance.TargetOffset);
            }
            else
            {
                Log.Debug("Placed BaseGhost is the first piece of a new base.");
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
