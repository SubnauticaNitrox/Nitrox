using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_ShowRotationControlsHint_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ShowRotationControlsHint", BindingFlags.Public | BindingFlags.Static);

        public static bool Prefix()
        {

            if (NitroxServiceLocator.LocateService<Building>().isInitialSyncing || NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {
                return false;
            }
            return true;

        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }

    }
}
