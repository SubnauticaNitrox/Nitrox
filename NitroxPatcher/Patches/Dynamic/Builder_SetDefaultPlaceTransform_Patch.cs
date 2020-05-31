using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Builder_SetDefaultPlaceTransform_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Builder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetDefaultPlaceTransform", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix(ref Vector3 position, ref Quaternion rotation)
        {
            NitroxServiceLocator.LocateService<Building>().Builder_SetDefaultPlaceTransform_Post(ref position, ref rotation);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
