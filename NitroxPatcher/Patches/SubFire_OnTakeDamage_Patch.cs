using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Hook onto <see cref="SubFire.OnTakeDamage(DamageInfo)"/>. If the function made it to the end, that means it created a new fire.
    /// </summary>
    class SubFire_OnTakeDamage_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubFire);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTakeDamage", BindingFlags.Instance | BindingFlags.Public);
        public static readonly OpCode FIRST_INJECTION_OPCODE = OpCodes.Ldloc_2;
        public static readonly OpCode SECOND_INJECTION_OPCODE = OpCodes.Call;

        /// <summary>
        /// When random number seeds are synced, we'll need to inject code after the index is generated. This injects just after the first line of code.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            bool firstOpCodeReached = false;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                
                if (instruction.opcode.Equals(FIRST_INJECTION_OPCODE))
                {
                    firstOpCodeReached = true;
                }

                if (firstOpCodeReached
                    && instruction.opcode.Equals(SECOND_INJECTION_OPCODE))
                {
                    // Multiplayer.Logic.Cyclops.FireCreated(gameObject, DamageInfo, CyclopsRooms)

                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Cyclops", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Component).GetMethod("get_gameObject", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_1);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_2);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Cyclops).GetMethod("OnFireCreated", BindingFlags.Public | BindingFlags.Instance));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
