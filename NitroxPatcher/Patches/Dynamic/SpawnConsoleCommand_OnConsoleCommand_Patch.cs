using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SpawnConsoleCommand_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnConsoleCommand t) => t.OnConsoleCommand_spawn(default(NotificationCenter.Notification)));

        private static readonly OpCode INJECTION_CODE = OpCodes.Call;
        private static readonly object INJECTION_OPERAND = Reflect.Method(() => Utils.CreatePrefab(default(GameObject), default(float), default(bool)));

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
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Callback(default(GameObject))));
                }

            }
        }

        public static void Callback(GameObject gameObject)
        {
            NitroxServiceLocator.LocateService<NitroxConsole>().Spawn(gameObject);
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
