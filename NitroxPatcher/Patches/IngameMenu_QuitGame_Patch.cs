using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public class IngameMenu_QuitGame_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QuitGame");

        public static void Prefix()
        {
            IMultiplayerSession multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            multiplayerSession.Disconnect();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
