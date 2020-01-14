﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches
{
    /*
     * Relays Cyclops FireSuppressionSystem to other players
     * This method was used instead of the OnClick to ensure, that the the suppression really started
     */
    class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsFireSuppressionSystemButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StartCooldown", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsFireSuppressionSystemButton __instance)
        {
            SubRoot cyclops = __instance.subRoot;
            NitroxId id = NitroxIdentifier.GetId(cyclops.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateFireSuppression(id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
