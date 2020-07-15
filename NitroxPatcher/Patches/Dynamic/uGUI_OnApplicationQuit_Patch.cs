using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class uGUI_OnApplicationQuit_Patch : NitroxPatch, IDynamicPatch
    {
        private readonly Type TYPE = typeof(uGUI);
        private const string TARGET_METHOD = "OnApplicationQuit";

        public static void Prefix()
        {
            IMultiplayerSession multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            multiplayerSession.Disconnect();
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TYPE.GetMethod(TARGET_METHOD, BindingFlags.NonPublic | BindingFlags.Instance));
        }
    }
}
