using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class DayNightCycle_OnConsoleCommand_night_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DayNightCycle);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnConsoleCommand_night", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix()
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetSender.Send(new ServerCommand("time night"));
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
