using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Radio_PlayRadioMessage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.PlayRadioMessage());

    public static void Prefix()
    {
        Resolve<IPacketSender>().Send(new RadioPlayPendingMessage());
    }
}
