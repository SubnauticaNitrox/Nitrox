using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
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

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
