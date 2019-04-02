﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class CyclopsHornButton_OnPress_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHornButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPress", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHornButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateHorn(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
