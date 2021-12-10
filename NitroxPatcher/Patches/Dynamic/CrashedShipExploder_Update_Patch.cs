using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CrashedShipExploder_Update_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashedShipExploder t) => t.Update());

        private static readonly OpCode INJECTION_OPCODE = OpCodes.Ldsfld;
        private static readonly object INJECTION_OPERAND = Reflect.Field(() => DayNightCycle.main);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            /* We add another check to prevent any explosion triggered by the client itself
             *
             * if ((UnityEngine.Object)DayNightCycle.main != (UnityEngine.Object)null)
             * 
             * =>
             * 
             * if ((UnityEngine.Object) DayNightCycle.main != (UnityEngine.Object) null && CrashedShipExploder_Update_Patch.GetCrashedUpdate())
             *
             */
            bool insideCondition = false;
            // If we don't only add it once, the event will repeat many times
            bool addedOnce = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    insideCondition = true;
                }
                if (!addedOnce && insideCondition && instruction.opcode.Equals(OpCodes.Brfalse))
                {
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => GetCrashedUpdate()));
                    yield return instruction;
                    addedOnce = true;
                }
            }
        }

        private static bool GetCrashedUpdate()
        {
            return Resolve<PDAManagerEntry>().AuroraExplosionTriggered;
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
