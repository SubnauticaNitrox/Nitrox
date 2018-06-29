using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class IngameMenuQuitConfirmation_OnYes_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenuQuitConfirmation);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnYes", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix()
        {
            LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            if (localPlayer != null)
            {
                localPlayer.Shutdown = true;
            }
        
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
