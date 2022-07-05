using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructableBase_SetState_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ConstructableBase t) => t.SetState(default, default));
        internal static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        internal static readonly object INJECTION_OPERAND = Reflect.Method(() => GameObject.Destroy(default));

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            List<CodeInstruction> instructionsList = new(instructions);
            // There are 2 occurences of this, the first one is the place we want to make the addition
            bool addedOnce = false;
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                /*
                 * Make it become
				 * if (Builder.CanDestroyObject(gameObject))
				 * {
				 *     Constructable_SetState_Patch.Callback(gameObject); <==========
				 *     UnityEngine.Object.Destroy(gameObject);
				 * }
                 */
                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND) && !addedOnce)
                {
                    addedOnce = true;
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => BeforeDestroy(default)));
                    // We can then copy the previous ldloc that was used 1 line before
                    yield return instructionsList[i - 1];
                }
                yield return instruction;
            }
        }

        public static void BeforeDestroy(GameObject gameObject)
        {
            if (NitroxEntity.TryGetEntityFrom(gameObject, out NitroxEntity nitroxEntity))
            {
                Resolve<IPacketSender>().Send(new EntityDestroy(nitroxEntity.Id));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
