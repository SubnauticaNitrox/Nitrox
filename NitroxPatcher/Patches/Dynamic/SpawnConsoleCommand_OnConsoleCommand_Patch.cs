using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SpawnConsoleCommand_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(SpawnConsoleCommand).GetMethod("OnConsoleCommand_spawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_CODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(Utils).GetMethod("CreatePrefab", BindingFlags.Public | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                /*
                 * GameObject gameObject = global::Utils.CreatePrefab(prefabForTechType, maxDist, i > 0);
                 * -> SpawnConsoleCommand_OnConsoleCommand_Patch.Callback(gameObject);
                 * LargeWorldEntity.Register(gameObject);
                 * CrafterLogic.NotifyCraftEnd(gameObject, techType);
                 * gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
                 */
                if (instruction.opcode == INJECTION_CODE && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, typeof(SpawnConsoleCommand_OnConsoleCommand_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }

            }
        }

        public static void Callback(GameObject gameObject)
        {
            NitroxServiceLocator.LocateService<NitroxConsole>().Spawn(gameObject);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
