using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Poop_Perform_Patch : NitroxPatch, IDynamicPatch
{
#if SUBNAUTICA
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Poop t) => t.Perform(default, default, default));
#elif BELOWZERO
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Poop t) => t.Perform(default, default));
#endif

    /*
     *     UnityEngine.Object.Instantiate<GameObject>(this.recourcePrefab, this.recourceSpawnPoint.position, this.recourceSpawnPoint.rotation);
     *     Poop_Perform_Patch.BroadcastPoop(this);   <--- INSERTED LINE
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).End()
                                            .Advance(-1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastPoop(default, default)))
                                            ])
                                            // Remove the pop instruction which is useless because we consumed the GameObject with the call above
                                            .RemoveInstruction()
                                            .InstructionEnumeration();
    }

    public static void BroadcastPoop(GameObject poopObject, Poop poop)
    {
        if (!poop.TryGetNitroxId(out NitroxId creatureId))
        {
            return;
        }
        Resolve<Items>().Dropped(poopObject);
        Resolve<IPacketSender>().Send(new CreaturePoopPerformed(creatureId));
    }
}
