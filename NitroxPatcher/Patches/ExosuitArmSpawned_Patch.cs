using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class ExosuitArmSpawned_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Exosuit);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdateColliders", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Exosuit __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastSpawnedArm(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

