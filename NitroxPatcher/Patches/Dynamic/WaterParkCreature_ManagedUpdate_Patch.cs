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
/// Syncs fish deletion once it ages to adult state.
/// </summary>
public sealed partial class WaterParkCreature_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WaterParkCreature t) => t.ManagedUpdate());

    /*
     * MODIFIED:
     * WaterParkCreature.Born(adultPrefab, this.currentWaterPark, base.transform.position);
     * base.SetWaterPark(null);
     * WaterParkCreature_ManagedUpdate_Patch.CreatureDestroyedCallback(this); <--- INSERTED
     * UnityEngine.Object.Destroy(base.gameObject);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Ldnull),
                                                new CodeMatch(OpCodes.Call, Reflect.Method((WaterParkItem t) => t.SetWaterPark(default))),
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => CreatureDestroyedCallback(default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void CreatureDestroyedCallback(WaterParkCreature waterParkCreature)
    {
        if (!waterParkCreature.TryGetNitroxId(out NitroxId creatureId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return;
        }

        Resolve<IPacketSender>().Send(new EntityDestroyed(creatureId));
    }
}
