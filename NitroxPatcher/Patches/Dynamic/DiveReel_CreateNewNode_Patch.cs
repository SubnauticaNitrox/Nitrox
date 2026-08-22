using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts a live DiveReel (Pathfinder Tool) node placement for the local player.
/// CreateNewNode also runs with loadingNode=true when restoring nodePositions from the
/// player's own save file on load (DiveReel.OnProtoDeserialize) -- that path must NOT be
/// broadcast as a new placement, only genuine live user actions (loadingNode=false, from
/// OnToolUseAnim) should ever produce a packet.
/// </summary>
public sealed partial class DiveReel_CreateNewNode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DiveReel t) => t.CreateNewNode(default, default, default));

    public static void Postfix(DiveReel __instance, Vector3 createPos, bool isFirst, bool loadingNode)
    {
        if (loadingNode)
        {
            return;
        }
        if (!Resolve<LocalPlayer>().PlayerId.HasValue)
        {
            return;
        }
        Resolve<IPacketSender>().Send(new DiveReelNodePlaced(Resolve<LocalPlayer>().PlayerId.Value, createPos.ToDto(), isFirst));
    }
}
