﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsExternalDamageManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CreatePoint", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(CyclopsExternalDamageManager __instance, out bool __state)
        {
            // Block from creating points if they aren't the owner of the sub
            __state = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(NitroxIdentifier.GetId(__instance.subRoot.gameObject));

            return __state;
        }

        public static void Postfix(CyclopsExternalDamageManager __instance, bool __state)
        {
            if (__state)
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnCreateDamagePoint(__instance.subRoot);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
