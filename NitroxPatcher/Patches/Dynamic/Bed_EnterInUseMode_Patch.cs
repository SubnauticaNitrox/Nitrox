using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Intercepts bed entry to send a packet and register with SleepManager.
/// Uses Prefix instead of Transpiler because we need to prevent the original method from
/// starting the sleep animation - in multiplayer we wait for all players before sleeping.
/// </summary>
public sealed partial class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.EnterInUseMode(default(Player)));

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

        Resolve<IPacketSender>().Send(new BedEnter());
        Resolve<SleepManager>().LocalPlayerEnteredBed(__instance);

        return false;
    }
}
