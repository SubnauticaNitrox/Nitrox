using System;
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

/// <summary>
/// Broadcasts the FireExtinguisherHolder's metadata when stored and the destruction of the extinguisher entity that is placed onto it.
/// </summary>
public sealed partial class FireExtinguisherHolder_TryStoreTank_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((FireExtinguisherHolder t) => t.TryStoreTank());

    public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
    public static readonly object INJECTION_OPERAND = Reflect.Method((GameObject gameObject) => gameObject.SetActive(default(bool)));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                /*
                 * Injects:  Callback(this, pickupable);
                 *
                 * This needs to be done before the pickupable object is destroyed because it has necessary metadata such as the tank fuel level.
                 */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Call, ((Action<FireExtinguisherHolder, Pickupable>)Callback).Method);
            }
        }
    }

    public static void Callback(FireExtinguisherHolder holder, Pickupable pickupable)
    {
        if (holder.TryGetIdOrWarn(out NitroxId holderId))
        {
            Resolve<Entities>().EntityMetadataChanged(holder, holderId);
        }

        if (pickupable.TryGetIdOrWarn(out NitroxId pickupableId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(pickupableId));
        }
    }
}
