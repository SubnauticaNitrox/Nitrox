using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitSubscreen_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((IngameMenu t) => t.QuitSubscreen());

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool foundRet = false;
        foreach (CodeInstruction instruction in instructions)
        {
            if (foundRet)
            {
                yield return instruction;
            }
#if SUBNAUTICA
            if (instruction.opcode == OpCodes.Ret)
#elif BELOWZERO
            if (instruction.opcode == OpCodes.Br)
#endif
            {
                foundRet = true;
            }
        }
    }
}
