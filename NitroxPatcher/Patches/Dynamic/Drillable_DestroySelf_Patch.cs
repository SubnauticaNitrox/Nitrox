using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Drillable_DestroySelf_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Drillable t) => t.DestroySelf());

    public static void Prefix(Drillable __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }
}
