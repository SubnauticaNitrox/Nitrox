using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(IngameMenu);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnSelect");
        public static readonly MethodInfo GAMEMODEUTILS_ISPERMADEATH_METHOD = typeof(GameModeUtils).GetMethod("IsPermadeath", BindingFlags.Public | BindingFlags.Static);

        public static void Postfix()
        {
            IngameMenu.main.saveButton.gameObject.SetActive(false);
            IngameMenu.main.quitToMainMenuButton.interactable = true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            /* Early return cuts out
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
             * if (PlatformUtils.isXboxOnePlatform)
		     * {
			 *      this.helpButton.gameObject.SetActive(true);
		     * }
             */
            foreach (CodeInstruction instruction in instructions)
            {
                if(GAMEMODEUTILS_ISPERMADEATH_METHOD.Equals(instruction.operand))
                {
                    yield return new CodeInstruction(OpCodes.Ret);
                    break;
                }

                yield return instruction;
            } 
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
