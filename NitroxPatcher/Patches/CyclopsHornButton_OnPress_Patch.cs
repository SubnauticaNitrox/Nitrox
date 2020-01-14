﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches
{
    public class CyclopsHornButton_OnPress_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHornButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPress", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHornButton __instance)
        {
            NitroxId id = NitroxIdentifier.GetId(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateHorn(id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
