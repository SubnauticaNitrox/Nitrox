using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class uGUI_OnApplicationQuit_Patch : NitroxPatch, IDynamicPatch
    {
        private readonly Type TARGET_TYPE = typeof(uGUI);
        private readonly string TARGET_METHOD = TARGET_TYPE.GetMethod("OnApplicationQuit", BindingFlags.NonPublic | BindingFlags.Instance);

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
