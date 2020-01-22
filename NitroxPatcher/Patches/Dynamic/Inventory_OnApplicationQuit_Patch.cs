using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Inventory_OnApplicationQuit_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Inventory);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnApplicationQuit", BindingFlags.NonPublic | BindingFlags.Instance);

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
