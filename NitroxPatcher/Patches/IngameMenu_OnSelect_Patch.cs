using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

namespace NitroxPatcher.Patches
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnSelect");
        public static readonly OpCode START_CUT_CODE = OpCodes.Call;
        public static readonly MethodInfo GAMEMODEUTILS_ISPERMADEATH_METHOD = typeof(GameModeUtils).GetMethod("IsPermadeath", BindingFlags.Public | BindingFlags.Static);
        public static readonly OpCode END_CUT_CODE = OpCodes.Call;
        public static readonly MethodInfo PLATFORMUTILS_GET_ISCONSOLEPLATFORM_METHOD = typeof(PlatformUtils).GetMethod("get_isConsolePlatform", BindingFlags.Public | BindingFlags.Static);

        public static void Postfix()
        {
            IngameMenu.main.saveButton.gameObject.SetActive(false);
            IngameMenu.main.quitToMainMenuButton.interactable = true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            int startCut = 0;
            int endCut = instructionList.Count-1;
            /* Cut out
             * if (GameModeUtils.IsPermadeath())
		     * {
			 *    this.quitToMainMenuText.text = Language.main.Get("SaveAndQuitToMainMenu");
			 *    this.saveButton.gameObject.SetActive(false);
		     * }
		     * else
		     * {
			 *     this.saveButton.interactable = this.GetAllowSaving();
			 *     this.quitToMainMenuButton.interactable = true;
		     * }
             */
            for (int i = 1; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode.Equals(START_CUT_CODE) && instructionList[i].operand.Equals(GAMEMODEUTILS_ISPERMADEATH_METHOD))
                {
                    startCut = i;
                }
                if (instructionList[i].opcode.Equals(END_CUT_CODE) && instructionList[i].operand.Equals(PLATFORMUTILS_GET_ISCONSOLEPLATFORM_METHOD))
                {
                    endCut = i-1;
                }
            }

            instructionList.RemoveRange(startCut, endCut);

            return instructionList;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
