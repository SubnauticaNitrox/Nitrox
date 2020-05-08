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

        public static readonly OpCode START_CUT_CODE = OpCodes.Call;
        public static readonly MethodInfo CUT_METHOD = typeof(GameModeUtils).GetMethod(nameof(GameModeUtils.IsPermadeath), BindingFlags.Public | BindingFlags.Static);
        public static readonly OpCode END_CUT_CODE = OpCodes.Ret;
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator ilGenerator, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            Label jmpLabel = ilGenerator.DefineLabel();
            int startCut = 0;
            int endCut = instructionList.Count;
            /**
            * Cuts out
            * if (GameModeUtils.IsPermadeath())
            * {
            *      SaveLoadManager.main.ClearSlotAsync(SaveLoadManager.main.GetCurrentSlot());
            *      this.EndGame();
            *      return;
            * }
            *    
            */
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode.Equals(OpCodes.Br))
                {
                    instructionList[i] = new CodeInstruction(OpCodes.Brtrue, jmpLabel);
                }
                else if (instruction.opcode.Equals(START_CUT_CODE) && instruction.operand.Equals(CUT_METHOD))
                {
                    startCut = i;
                }
                else if (endCut == instructionList.Count && instruction.opcode.Equals(END_CUT_CODE))
                {
                    endCut = (i + 1) - startCut;
                    instructionList[i + 1].labels.RemoveAt(0);
                    instructionList[i + 1].labels.Add(jmpLabel);
                }

            }
            instructionList.RemoveRange(startCut, endCut);
            return instructionList;
        }

        public static bool Prefix(Player __instance)
        {
            Log.Debug("OnKill event has been triggered");
            return true;
        }

        public static void Postfix(Player __instance)
        {
            Log.Debug("OnKill event is now over");
        }

        public override void Patch(HarmonyInstance harmony)
        {
            HarmonyInstance.DEBUG = true;
            PatchMultiple(harmony, TARGET_METHOD, true, false, true);
            //PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
