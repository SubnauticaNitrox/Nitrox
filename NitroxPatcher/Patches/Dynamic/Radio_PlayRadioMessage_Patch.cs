using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Radio_PlayRadioMessage_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.PlayRadioMessage());

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
