using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Radio_PlayRadioMessage_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Radio);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("PlayRadioMessage", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix()
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetSender.Send(new RadioPlayPendingMessage());
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
