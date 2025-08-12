using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public partial class MainGameController_StartGame_Patch : NitroxPatch, IPersistentPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((MainGameController t) => t.StartGame()));

    public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
    public static readonly object INJECTION_OPERAND = Reflect.Method(() => WaitScreen.Remove(default));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);

        int injectSeenCounter = 0;

        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;


            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                injectSeenCounter++;

#if SUBNAUTICA
                if (injectSeenCounter == 3)
#elif BELOWZERO
                if (injectSeenCounter == 2)
#endif
                {
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Multiplayer.SubnauticaLoadingCompleted()));
                }
            }
        }
    }
}
