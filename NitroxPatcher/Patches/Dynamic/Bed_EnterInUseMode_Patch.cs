using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((Bed t) => t.EnterInUseMode(default(Player)));

    public static void Postfix()
    {
        Resolve<IPacketSender>().Send(new BedEnter());
    }
}
