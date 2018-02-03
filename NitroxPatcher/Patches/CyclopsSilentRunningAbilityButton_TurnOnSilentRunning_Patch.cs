﻿using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxClient.GameLogic.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsSilentRunningAbilityButton_TurnOnSilentRunning_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSilentRunningAbilityButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TurnOnSilentRunning", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsSilentRunningAbilityButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            Multiplayer.Logic.Cyclops.BeginSilentRunning(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
