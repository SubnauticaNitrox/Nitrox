using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Radio_PlayRadioMessage_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((Radio t) => t.PlayRadioMessage());

    public static void Prefix()
    {
        Resolve<IPacketSender>().Send(new RadioPlayPendingMessage());
    }
}
