using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class DayNightCycle_OnConsoleCommand_night_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((DayNightCycle t) => t.OnConsoleCommand_night(default(NotificationCenter.Notification)));

    public static bool Prefix()
    {
        Resolve<IPacketSender>().Send(new ServerCommand("time night"));
        return false;
    }
}
