using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.SetTabs(default));

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Blt;

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = instructions.ToList();

        foreach (CodeInstruction instruction in instructionList)
        {
            yield return instruction;
            if (instruction.opcode.Equals(INJECTION_OPCODE))
            {
                /*
                 * Insert
                 * uGUI_PDA_Initialize_Patch.SetupNitroxIcons(this, array);
                 * right before
                 * uGUI_Toolbar uGUI_Toolbar = this.toolbar;
                 */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => SetupNitroxIcons(default, default)));
            }
        }
    }

    public static void SetupNitroxIcons(uGUI_PDA __instance, Atlas.Sprite[] array)
    {
        NitroxPDATabManager nitroxTabManager = Resolve<NitroxPDATabManager>();
        List<NitroxPDATab> customTabs = new(nitroxTabManager.CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            // Array index must be fixed so that the callback is executed with its precise value
            int arrayIndex = array.Length - i - 1;
            int tabIndex = customTabs.Count - i - 1;

            string tabIconAssetName = customTabs[tabIndex].TabIconAssetName;
            if (!nitroxTabManager.TryGetTabSprite(tabIconAssetName, out Atlas.Sprite sprite))
            {
                nitroxTabManager.SetSpriteLoadedCallback(tabIconAssetName, callbackSprite => AssignSprite(__instance.toolbar, arrayIndex, callbackSprite));
                // Take the fallback icon from another tab
                sprite = SpriteManager.Get(SpriteManager.Group.Tab, $"Tab{customTabs[tabIndex].FallbackTabIcon}");
            }
            array[arrayIndex] = sprite;
        }
    }

    private static void AssignSprite(uGUI_Toolbar toolbar, int index, Atlas.Sprite sprite)
    {
        toolbar.icons[index].SetForegroundSprite(sprite);
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
