using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.OnSelect(default(bool)));
        private static readonly MethodInfo IS_PERMA_DEATH_METHOD = Reflect.Method(() => GameModeUtils.IsPermadeath());

        public static void Postfix()
        {
            IngameMenu.main.saveButton.gameObject.SetActive(false);
            IngameMenu.main.quitToMainMenuButton.interactable = true;

#if DEBUG
            IngameMenu.main.ActivateDeveloperMode(); // Activating it here to ensure IngameMenu is ready for it
#endif
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
                if (IS_PERMA_DEATH_METHOD.Equals(instruction.operand))
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
