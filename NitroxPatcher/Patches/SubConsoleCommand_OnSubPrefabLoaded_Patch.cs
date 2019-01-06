using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Called whenever a Cyclops or Seamoth is spawned. I only tested for the "sub cyclops" command, so it may not work for other methods of construction.
    /// </summary>
    class SubConsoleCommand_OnSubPrefabLoaded_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubConsoleCommand);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnSubPrefabLoaded", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(SubConsoleCommand __instance)
        {
            Vector3 spawnPosition = (Vector3)__instance.ReflectionGet("spawnPosition");
            Quaternion spawnRotation = (Quaternion)__instance.ReflectionGet("spawnRotation");

            NitroxServiceLocator.LocateService<Cyclops>().SpawnNew(__instance.GetLastCreatedSub(), spawnPosition, spawnRotation);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
