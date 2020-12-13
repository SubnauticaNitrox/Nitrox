using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Core;

namespace Nitrox.Patcher.Patches.Dynamic
{
    internal class uGUI_OnApplicationQuit_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly Type TARGET_TYPE = typeof(uGUI);
        private static readonly MethodInfo TARGET_METHOD = TARGET_TYPE.GetMethod("OnApplicationQuit", BindingFlags.NonPublic | BindingFlags.Instance);

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
