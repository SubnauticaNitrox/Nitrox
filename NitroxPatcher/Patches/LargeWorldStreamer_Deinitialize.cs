using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NitroxHarmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public class LargeWorldStreamer_Deinitialize_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LargeWorldStreamer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deinitialize", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix()
        {
            IMultiplayerSession multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();

            if (multiplayerSession.Client.IsConnected)
            {
                multiplayerSession.Disconnect();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
