using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class LaunchRocket_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((LaunchRocket t) => t.OnHandClick(default));

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
    internal static readonly object INJECTION_OPERAND = Reflect.Method(() => LaunchRocket.SetLaunchStarted());
    
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);
        /* We replace
         * 
         * LaunchRocket.SetLaunchStarted();
         * 
         * by
         * 
         * LaunchRocket_OnHandClick_Patch.RequestRocketLaunch()
         * return; (by just removing the following instructions)
         */
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                // We must transfer the labels from the previous instruction
                CodeInstruction loadInstruction = new(OpCodes.Ldarg_0);
                loadInstruction.labels = instruction.labels;
                yield return loadInstruction;
                yield return new(OpCodes.Call, Reflect.Method(() => RequestRocketLaunch(default)));
                yield return new(OpCodes.Ret);
                break;
            }
            yield return instruction;
        }
    }

    public static void RequestRocketLaunch(LaunchRocket launchRocket)
    {
        Rocket rocket = launchRocket.RequireComponentInParent<Rocket>();
        Resolve<Rockets>().RequestRocketLaunch(rocket);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
