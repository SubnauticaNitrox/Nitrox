using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class TimeCapsule_Open_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((TimeCapsule t) => t.Open());

    public static void Prefix(TimeCapsule __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }
}
