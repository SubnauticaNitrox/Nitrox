using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class DayNightCycle_OnConsoleCommand_day_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.OnConsoleCommand_day(default(NotificationCenter.Notification)));

    public static bool Prefix()
    {
        IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        packetSender.Send(new ServerCommand("time day"));
        return false;
    }
}
