using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using UnityEngine;
using static System.Reflection.Emit.OpCodes;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class DevConsole_Update_Patch : NitroxPatch, IDynamicPatch
    {
        /// <summary>
        ///     Pattern to keep DevConsole disabled when enter is pressed.
        /// </summary>
        public static readonly InstructionsPattern DevConsoleSetStateTruePattern = new()
        {
            InstructionPattern.Call(nameof(Input), nameof(Input.GetKeyDown)),
            Brfalse,
            Ldarg_0,
            Ldfld,
            Brtrue,
            Ldarg_0,
            { Ldc_I4_1, "TrueArgument" },
            InstructionPattern.Call(nameof(DevConsole), nameof(DevConsole.SetState))
        };

        public static readonly MethodInfo TargetMethod = Reflect.Method((DevConsole t) => t.Update());

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions) => instructions
            .Transform(DevConsoleSetStateTruePattern, (label, instruction) =>
            {
                switch (label)
                {
                    case "TrueArgument":
                        instruction.opcode = Ldc_I4_0;
                        break;
                }
            });

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TargetMethod);
        }
    }
}
