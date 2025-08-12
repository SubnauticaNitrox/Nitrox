using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LaunchRocket_OnHandClick_Patch : NitroxPatch, IDynamicPatch
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
                yield return new CodeInstruction(OpCodes.Ldarg_0)
                {
                    labels = instruction.labels
                };
                yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => RequestRocketLaunch(default)));
                yield return new CodeInstruction(OpCodes.Ret);
                break;
            }
            yield return instruction;
        }
    }

    private static void RequestRocketLaunch(LaunchRocket launchRocket)
    {
        Rocket rocket = launchRocket.RequireComponentInParent<Rocket>();
        Resolve<Rockets>().RequestRocketLaunch(rocket);
    }
}
