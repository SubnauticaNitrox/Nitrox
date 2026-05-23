using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Handles bed entry for multiplayer sleep coordination.
/// Bed lie-down animations ARE synced through the cinematic system.
/// This patch manages the sleep state and multiplayer coordination after the animation completes.
/// </summary>
public sealed partial class Bed_EnterInUseMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Bed t) => t.EnterInUseMode(default(Player)));

    public static void Prefix(Bed __instance)
    {
        if (__instance.inUseMode != Bed.InUseMode.None)
        {
            return;
        }

        // Send packet to notify other players for sleep coordination
        Resolve<IPacketSender>().Send(new BedEnter());
        Resolve<SleepManager>().EnterBed(__instance);
    }
}
