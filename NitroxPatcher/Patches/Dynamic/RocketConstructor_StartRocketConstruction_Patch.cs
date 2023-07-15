using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class RocketConstructor_StartRocketConstruction_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((RocketConstructor t) => t.StartRocketConstruction());
    private static readonly OpCode INJECTION_CODE = OpCodes.Stloc_2;

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            /* if (this.crafterLogic.Craft(currentStageTech, craftTime)) {
             *      GameObject toBuild = this.rocket.StartRocketConstruction();
             *  ->  RocketConstructor_StartRocketConstruction_Patch.Callback(this.rocket);
             *      ItemGoalTracker.OnConstruct(currentStageTech);
             *      this.SendBuildBots(toBuild);
             * }
             */
            if (instruction.opcode == INJECTION_CODE)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                yield return new CodeInstruction(OpCodes.Ldfld, Reflect.Field((RocketConstructor t) => t.rocket)); // this.rocket
                yield return new CodeInstruction(OpCodes.Call, ((Action<Rocket>)Callback).Method);
            }
        }
    }

    private static void Callback(Rocket rocket)
    {
        if (rocket.TryGetIdOrWarn(out NitroxId rocketId))
        {
            Resolve<Entities>().EntityMetadataChanged(rocket, rocketId);
        }
    }
}
