using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Add custom tabs to the PDA by injecting them in the regular list before the actual initialization.
/// </summary>
public class uGUI_PDA_Initialize_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.Initialize());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
    internal static readonly object INJECTION_OPERAND = Reflect.Field((uGUI_PDA t) => t.tabs);

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        Validate.NotNull(INJECTION_OPERAND);

        List<CodeInstruction> instructionList = instructions.ToList();

        foreach (CodeInstruction instruction in instructionList)
        {
            yield return instruction;
            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                /*
                 * Insert
                 * uGUI_PDA_Initialize_Patch.InjectNitroxTabs(this);
                 * right before
                 * foreach (KeyValuePair<PDATab, uGUI_PDATab> keyValuePair in this.tabs)
                 */
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => InjectNitroxTabs(default)));
            }
        }
    }

    public static void InjectNitroxTabs(uGUI_PDA __instance)
    {
        // Initialize all the custom tabs so that they can create their required components
        // And add their "types" to the tab list
        foreach (KeyValuePair<PDATab, NitroxPDATab> nitroxTab in Resolve<NitroxPDATabManager>().CustomTabs)
        {
            nitroxTab.Value.OnInitializePDA(__instance);
            uGUI_PDA.regularTabs.Add(nitroxTab.Key);
            __instance.tabs.Add(nitroxTab.Key, nitroxTab.Value.uGUI_PDATab);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
