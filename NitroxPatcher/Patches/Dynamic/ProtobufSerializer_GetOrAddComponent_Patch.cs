using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches.Dynamic
{
    class ProtobufSerializer_GetOrAddComponent_Patch : NitroxPatch, IDynamicPatch
    {

        MethodInfo method = typeof(ProtobufSerializer).GetMethod("GetOrAddComponent", BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            List<CodeInstruction> instrList = instructions.ToList();

            Type[] parameters = new Type[]
            {
                typeof(UnityEngine.Object),
                typeof(string),
                typeof(object[])
            };

            Label newJmp = ilGenerator.DefineLabel();

            MethodInfo logWarningFormat = typeof(UnityEngine.Debug).GetMethod("LogWarningFormat", BindingFlags.Public | BindingFlags.Static, null, parameters, null);

            for (int i = 0; i < instrList.Count; i++)
            {
                if (i + 19 < instrList.Count && logWarningFormat.Equals(instrList[i + 19].operand))
                {
                    i += 19; // Skips over LogWarningFormat("Adding Component")
                }
                else if (i + 1 < instrList.Count && "BLACKLISTED COMPONENT".Equals(instrList[i + 1].operand))
                {
                    yield return new CodeInstruction(instrList[i].opcode, newJmp);
                }
                else
                {
                    if ("AddComponent-".Equals(instrList[i].operand))
                    {
                        instrList[i].labels.Add(newJmp);
                    }

                    yield return instrList[i];
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, method);
        }
    }
}
