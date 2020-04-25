using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Bed).GetMethod("EnterInUseMode", BindingFlags.Instance | BindingFlags.NonPublic);

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
