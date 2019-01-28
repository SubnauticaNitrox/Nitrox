using System;
using Harmony;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches
{
    class DayNightCycle_OnConsoleCommand_night_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DayNightCycle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnConsoleCommand_night", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix()
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetSender.Send(new ServerCommand("night"));
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
