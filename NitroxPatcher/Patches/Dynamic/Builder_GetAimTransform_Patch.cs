using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_GetAimTransform_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("GetAimTransform", BindingFlags.Public | BindingFlags.Static);

        public static void Postfix(ref Transform __result)
        {
            NitroxServiceLocator.LocateService<Building>().Builder_GetAimTransform_Post(ref __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
