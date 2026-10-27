using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the attack triggered by grabbing a <see cref="Crash"/> with the Propulsion Cannon and releasing it
/// </summary>
public sealed partial class Crash_OnRelease_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method<Crash>(t => ((IPropulsionCannonAmmo)t).OnRelease());

    public static void Postfix(Crash __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) || !Resolve<SimulationOwnership>().HasAnyLockType(creatureId) ||
            !Player.main.TryGetIdOrWarn(out NitroxId targetId))
        {
            return;
        }

        Resolve<IPacketSender>().Send(new CrashAttackLastTarget(creatureId, targetId));
    }
}
