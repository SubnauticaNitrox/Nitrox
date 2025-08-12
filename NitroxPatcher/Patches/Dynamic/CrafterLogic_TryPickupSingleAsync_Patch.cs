using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/**
 * When a player is finished crafting an item, we need to let the server know we spawned the items.  We also
 * let other players know to close out the crafter and consider it empty.
 */
public sealed partial class CrafterLogic_TryPickupSingleAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((CrafterLogic t) => t.TryPickupSingleAsync(default(TechType), default(IOut<bool>))));

    public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
    public static readonly object INJECTION_OPERAND = Reflect.Method(() => CrafterLogic.NotifyCraftEnd(default(GameObject), default(TechType)));

    private static readonly MethodInfo COMPONENT_GAMEOBJECT_GETTER = Reflect.Property((Component t) => t.gameObject).GetMethod;

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                /*
                 * Injects:  Callback(this.gameObject, gameObject);
                 */
                yield return original.Ldloc<CrafterLogic>();
                yield return new CodeInstruction(OpCodes.Callvirt, COMPONENT_GAMEOBJECT_GETTER);
                yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)5);
                yield return new CodeInstruction(OpCodes.Call, ((Action<GameObject, GameObject>)Callback).Method);
            }
        }
    }

    public static void Callback(GameObject crafter, GameObject item)
    {
        if (crafter.TryGetIdOrWarn(out NitroxId crafterId))
        {
            // Tell the other players to consider this crafter to no longer contain a tech type.
            Resolve<Entities>().BroadcastMetadataUpdate(crafterId, new CrafterMetadata(null, DayNightCycle.main.timePassedAsFloat, 0));
        }

        // The Pickup() item codepath will inform the server that the item was added to the inventory.
    }
}
