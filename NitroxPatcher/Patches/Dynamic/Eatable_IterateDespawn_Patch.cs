using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="Eatable.IterateDespawn"/> from happening on non-simulated entities and broadcast it for simulated entities
/// </summary>
public sealed partial class Eatable_IterateDespawn_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Eatable t) => t.IterateDespawn());

    public static bool Prefix(CreatureDeath __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }
        return false;
    }

    /*
     * if (DayNightCycle.main.timePassedAsFloat - this.timeDespawnStart > this.despawnDelay)
     * {
     *     base.CancelInvoke();
     *     UnityEngine.Object.Destroy(base.gameObject);
     *     Eatable_IterateDespawn_Patch.BroadcastEatableDestroyed(this);      [INSERTED LINE]
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // We add instructions right before the ret which is equivalent to inserting at last offset
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                                            .Insert(new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastEatableDestroyed(default))))
                                            .InstructionEnumeration();
    }

    public static void BroadcastEatableDestroyed(Eatable eatable)
    {
        if (eatable.TryGetNitroxId(out NitroxId objectId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
        }
    }
}
