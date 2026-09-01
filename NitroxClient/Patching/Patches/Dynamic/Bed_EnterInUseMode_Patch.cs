using System;
using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Intercepts bed entry to send a packet and register with SleepManager.
/// Uses Prefix instead of Transpiler because we need to prevent the original method from
/// starting the sleep animation - in multiplayer we wait for all players before sleeping.
/// </summary>
internal sealed partial class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    private static IPacketSender packetSender;
    private static SleepManager sleepManager;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.EnterInUseMode(default(Player)));

    public Bed_EnterInUseMode_Patch(IPacketSender ps, SleepManager sm)
    {
        packetSender = ps ?? throw new ArgumentNullException(nameof(ps));
        sleepManager = sm ?? throw new ArgumentNullException(nameof(sm));
    }

    public static bool Prefix(Bed __instance, Player player)
    {
        if (__instance.inUseMode != Bed.InUseMode.None)
        {
            return false;
        }

        player.FreezeStats();
        player.cinematicModeActive = true;
        MainCameraControl.main.viewModel.localRotation = UnityEngine.Quaternion.identity;
        __instance.inUseMode = Bed.InUseMode.Sleeping;

        packetSender.Send(new BedEnter());
        sleepManager.EnterBed(__instance);

        return false;
    }
}
