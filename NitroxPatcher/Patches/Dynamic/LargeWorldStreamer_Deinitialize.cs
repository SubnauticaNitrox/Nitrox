using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class LargeWorldStreamer_Deinitialize_Patch : NitroxPatch, IDynamicPatch
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
