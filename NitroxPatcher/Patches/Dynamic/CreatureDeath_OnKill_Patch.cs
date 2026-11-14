using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="CreatureDeath.OnKill"/> from happening on entities without an id, while replaying a remote death, and on non-simulated entities
/// during initial sync or when replicating a remote health change.
/// </summary>
/// <remarks>
/// Creature health isn't synced so the player who brings a creature's health down to 0 can be the only one to know about its death,
/// even when they're not simulating it. Letting OnKill run makes them broadcast the death through
/// <see cref="CreatureDeath_OnKillAsync_Patch"/>, otherwise the creature would stay alive for everyone else.
/// </remarks>
public sealed partial class CreatureDeath_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureDeath t) => t.OnKill());

    public static bool Prefix(CreatureDeath __instance)
    {
        // CreatureDeath's part of a death which happened for another player is replicated separately (see RemoveCreatureCorpseProcessor)
        if (Resolve<LiveMixinManager>().IsReplayingRemoteKill)
        {
            return false;
        }

        if (!__instance.TryGetNitroxId(out NitroxId creatureId))
        {
            return false;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return false;
        }

        return !Resolve<LiveMixinManager>().IsRemoteHealthChanging;
    }
}
