using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class IngameMenu_OnSelect_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.OnSelect(default(bool)));
        private static readonly MethodInfo UPDATE_BUTTONS_METHOD = Reflect.Method((IngameMenu t) => t.UpdateButtons());

        public static void Postfix()
        {
            IngameMenu.main.saveButton.gameObject.SetActive(false);

#if DEBUG
            IngameMenu.main.ActivateDeveloperMode(); // Activating it here to ensure IngameMenu is ready for it
#endif
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            /* Early return cuts out
             * this.UpdateButtons();
             */
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                if (UPDATE_BUTTONS_METHOD.Equals(instructionList[i + 2].operand))
                {
                    yield return new CodeInstruction(OpCodes.Ret);
                    break;
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
