using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketConstructor_StartRocketConstruction_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(RocketConstructor).GetMethod("StartRocketConstruction", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_CODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = typeof(Rocket).GetMethod("StartRocketConstruction", BindingFlags.Public | BindingFlags.Instance);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode == INJECTION_CODE && instruction.operand.Equals(INJECTION_OPERAND))
                {

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, typeof(RocketConstructor_StartRocketConstruction_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }

            }
        }

        public static void Callback(TechType techType, GameObject rocket)
        {
            NitroxId nitroxId = NitroxEntity.GetId(rocket);

            Log.Info($"{nameof(RocketConstructor_StartRocketConstruction_Patch)}: Tried to broadcast update : RocketBase: {nitroxId}, ");

            NitroxServiceLocator.LocateService<Rockets>().BroadCastRocketStateUpdate(nitroxId, techType);

        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
