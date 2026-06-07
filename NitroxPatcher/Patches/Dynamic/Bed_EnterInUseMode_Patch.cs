using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Sends sleep coordination packet when entering bed sleep mode.
/// The bed animation packet is sent from Bed_OnHandClick_Patch.
/// Prevents the black fade from starting until all players are ready to sleep.
/// </summary>
public sealed partial class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.EnterInUseMode(default(Player)));

    public static void Prefix(Bed __instance)
    {
        // Send sleep coordination packet
        Resolve<IPacketSender>().Send(new BedEnter());
        Resolve<SleepManager>().EnterBed(__instance);
    }

    public static void Postfix()
    {
        // Stop the sleep screen that the game just started
        // We'll start it later when all players are ready (in SleepManager.OnAllPlayersSleeping)
        if (uGUI_PlayerSleep.main)
        {
            uGUI_PlayerSleep.main.StopSleepScreen();
        }
    }
}

