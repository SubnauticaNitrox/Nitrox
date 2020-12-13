using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Bed);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("EnterInUseMode", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix()
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            packetSender.Send(new BedEnter());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
