﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches
{
    class CyclopsDecoyLaunchButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsDecoyLaunchButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHornButton __instance)
        {
            NitroxId id = NitroxIdentifier.GetId(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastLaunchDecoy(id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
