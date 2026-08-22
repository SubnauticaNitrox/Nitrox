using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts a live DiveReel (Pathfinder Tool) trail reset for the local player.
/// </summary>
public sealed partial class DiveReel_ResetNodes_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DiveReel t) => t.ResetNodes());

    public static void Postfix(DiveReel __instance)
    {
        if (!Resolve<LocalPlayer>().PlayerId.HasValue)
        {
            return;
        }
        Resolve<IPacketSender>().Send(new DiveReelNodesReset(Resolve<LocalPlayer>().PlayerId.Value));
    }
}
