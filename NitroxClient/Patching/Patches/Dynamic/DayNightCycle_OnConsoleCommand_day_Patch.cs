using System;
using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class DayNightCycle_OnConsoleCommand_day_Patch : NitroxPatch, IDynamicPatch
{
    private static IPacketSender packetSender;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.OnConsoleCommand_day(default(NotificationCenter.Notification)));

    public DayNightCycle_OnConsoleCommand_day_Patch(IPacketSender ps)
    {
        packetSender = ps ?? throw new ArgumentNullException(nameof(ps));
    }

    public static bool Prefix()
    {
        packetSender.Send(new ServerCommand("time day"));
        return false;
    }
}
