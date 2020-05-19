using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CellManager_RegisterEntity_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RegisterEntity", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(GameObject) }, null);

        public static bool Prefix(CellManager __instance, GameObject ent)
        {
            return NitroxServiceLocator.LocateService<Building>().CellManager_RegisterEntity_Pre(ent);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
