using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class PinManager_NotifyMove_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((PinManager t) => t.NotifyMove(default, default));

    public static void Prefix()
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        // We can set a throttle as big as we want because the local player is the only one concerned
        Resolve<ThrottledPacketSender>().SendThrottled(new PinMoved(PinManager.main.pins.Select(techType => (int)techType).ToList()), 1);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}