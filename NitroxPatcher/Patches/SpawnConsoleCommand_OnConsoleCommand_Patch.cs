using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxPatcher.Patches
{
    public class SpawnConsoleCommand_OnConsoleCommand_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SpawnConsoleCommand);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnConsoleCommand_spawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(LargeWorldEntity).GetMethod("Register", BindingFlags.Public | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
         
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    yield return original.Ldloc<GameObject>(1);
                    yield return new CodeInstruction(OpCodes.Call, typeof(SpawnConsoleCommand_OnConsoleCommand_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }
            }
        }

        public static void Callback(GameObject gameObject)
        {
            NitroxServiceLocator.LocateService<DevConsoleEventEntry>().Spawn(gameObject);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

