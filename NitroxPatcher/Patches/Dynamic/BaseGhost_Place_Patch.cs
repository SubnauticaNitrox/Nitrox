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
            TransientLocalObjectManager.Remove(TransientObjectType.LATEST_FIRST_BASE_PIECE);
            TransientLocalObjectManager.Remove(TransientObjectType.LATEST_CONSTRUCTED_BASE);
            TransientLocalObjectManager.Remove(TransientObjectType.LATEST_CONSTRUCTED_BASE_CELL);
            
            // This may be the first piece of a base.  In that case, we don't have an existing base piece and 
            // should record this as the first piece of a base.
            if (__instance.TargetBase == null)
            {
                Log.Debug("Base ghost is the first piece to a base");
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_FIRST_BASE_PIECE, __instance.gameObject);
            }
            else
            {
                Log.Debug("Base ghost is a new piece of an existing base");

                // In this case, our piece is part of an existing, larger base.  We'll record the base and offset.  
                // We don't directly record the piece as it will be re-built when the base rebuilds geometry (this
                // happens each time a piece is built, subnautica destroys all others and replaces them).
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_CONSTRUCTED_BASE, __instance.TargetBase);
                TransientLocalObjectManager.Add(TransientObjectType.LATEST_CONSTRUCTED_BASE_CELL, __instance.TargetOffset);
            }

        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
