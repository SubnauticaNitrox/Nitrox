using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
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
#if SUBNAUTICA
                yield return new CodeInstruction(OpCodes.Call, ((Action<uGUI_PDA, Atlas.Sprite[]>)SetupNitroxIcons).Method);
#elif BELOWZERO
                yield return new CodeInstruction(OpCodes.Call, ((Action<uGUI_PDA, Sprite[]>)SetupNitroxIcons).Method);
#endif
            }
        }
    }

    public static void SetupNitroxIcons(uGUI_PDA __instance, Sprite[] array)
    {
        // In the case SetTabs is used with a null value (from TimeCapsule for example)
        if (array.Length == 0)
        {
            return;
        }

        NitroxPDATabManager nitroxTabManager = Resolve<NitroxPDATabManager>();
        List<NitroxPDATab> customTabs = new(nitroxTabManager.CustomTabs.Values);
        for (int i = 0; i < customTabs.Count; i++)
        {
            // Array index must be fixed so that the callback is executed with its precise value
            int arrayIndex = array.Length - i - 1;
            int tabIndex = customTabs.Count - i - 1;

            string tabIconAssetName = customTabs[tabIndex].TabIconAssetName;
            if (!nitroxTabManager.TryGetTabSprite(tabIconAssetName, out Sprite sprite))
            {
                nitroxTabManager.SetSpriteLoadedCallback(tabIconAssetName, callbackSprite => AssignSprite(__instance.toolbar, arrayIndex, callbackSprite));
                // Take the fallback icon from another tab
                sprite = SpriteManager.Get(SpriteManager.Group.Tab, $"Tab{customTabs[tabIndex].FallbackTabIcon}");
            }
            array[arrayIndex] = sprite;
        }
    }

    private static void AssignSprite(uGUI_Toolbar toolbar, int index, Sprite sprite)
    {
        if (index < toolbar.icons.Count)
        {
            toolbar.icons[index].SetForegroundSprite(sprite);
        }
    }
}
