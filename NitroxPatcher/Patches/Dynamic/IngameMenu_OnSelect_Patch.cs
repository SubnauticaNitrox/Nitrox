using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(IngameMenu).GetMethod("OnSelect");
        private static readonly MethodInfo GameModeUtilsIsPermadeathMethod = typeof(GameModeUtils).GetMethod("IsPermadeath", BindingFlags.Public | BindingFlags.Static);

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
                if (GameModeUtilsIsPermadeathMethod.Equals(instruction.operand))
                {
                    yield return new CodeInstruction(OpCodes.Ret);
                    break;
                }

                yield return instruction;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, targetMethod);
            PatchPostfix(harmony, targetMethod);
        }
    }
}
