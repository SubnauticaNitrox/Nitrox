using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches
{
    public class Radio_PlayRadioMessage_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Radio);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("PlayRadioMessage", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix()
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetSender.Send(new RadioPlayPendingMessage());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
