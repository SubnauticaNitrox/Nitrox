using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Player_OnKill_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Player).GetMethod(nameof(Player.OnKill), BindingFlags.Public | BindingFlags.Instance);

        public static readonly MethodInfo SKIP_METHOD = typeof(GameModeUtils).GetMethod(nameof(GameModeUtils.IsPermadeath), BindingFlags.Public | BindingFlags.Static);
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            /**
            * Skips
            * if (GameModeUtils.IsPermadeath())
            * {
            *      SaveLoadManager.main.ClearSlotAsync(SaveLoadManager.main.GetCurrentSlot());
            *      this.EndGame();
            *      return;
            * }
            */
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instr = instructionList[i];
                
                if (instr.opcode == OpCodes.Call && instr.operand.Equals(SKIP_METHOD))
                {
                    CodeInstruction newInstr = new CodeInstruction(OpCodes.Ldc_I4_0);
                    newInstr.labels = instr.labels;
                    yield return newInstr;
                }
                else
                {
                    yield return instr;
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
