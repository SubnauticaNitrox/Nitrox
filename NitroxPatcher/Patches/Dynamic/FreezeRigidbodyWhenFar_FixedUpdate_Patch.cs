using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FreezeRigidbodyWhenFar_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((FreezeRigidbodyWhenFar t) => t.FixedUpdate());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = instructions.ToList();
        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];

            if (instruction.opcode == OpCodes.Call && instruction.operand.Equals(Reflect.Method((Component c) => c.GetComponent<Rigidbody>())))
            {
                yield return instruction;
                yield return instructionList[i + 1]; // Yield the found instruction and the instruction after it
                object jmpLabel = null;

                for (int j = i; j < instructionList.Count; j++) // search for branch instruction
                {
                    if (instructionList[j].opcode == OpCodes.Ble_Un)
                    {
                        jmpLabel = instructionList[j].operand; // Copy original label
                        break;
                    }
                }

                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Call, Reflect.Property((Component c) => c.gameObject).GetGetMethod());
                yield return new(OpCodes.Call, Reflect.Method(() => IsMoving(default)));
                yield return new(OpCodes.Brtrue, jmpLabel); // Put down original jmp label

                i = i + 1;
                continue;
            }

            yield return instruction;
        }
    }

    public static bool IsMoving(GameObject go)
    {
        return go.TryGetComponent(out MultiplayerMovementController mc) && mc.Receiving;
    }
}
