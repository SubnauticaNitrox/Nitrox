using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class DayNightCycle_OnConsoleCommand_night_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(DayNightCycle).GetMethod("OnConsoleCommand_night", BindingFlags.Instance | BindingFlags.NonPublic);

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
