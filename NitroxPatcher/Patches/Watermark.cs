using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxPatcher.Patches
{
    [HarmonyPatch(typeof(uGUI_BuildWatermark))]
    [HarmonyPatch("UpdateText")]
    public class WaterMarkPatcher
    {
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Console.WriteLine("Executing watermark patch.");
            int count = 0;
            foreach(CodeInstruction instruction in instructions)
            {
                if(count == 16)
                {
                    instruction.operand = "\n Multiplayer loaded\n";
                }

                yield return instruction;
                count++;
            }
        }
    }
}
