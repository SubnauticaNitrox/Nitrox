using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// <see cref="CyclopsExternalDamageManager.CreatePoint()"/> is at the bottom of the execution list during the <see cref="CyclopsDamagePoint"/> creation.
    /// <see cref="CyclopsExternalDamageManager.OnTakeDamage"/> is where the logic starts.
    /// </summary>
    class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsExternalDamageManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CreatePoint", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsExternalDamageManager __instance)
        {
            Multiplayer.Logic.Cyclops.UpdateExternalDamagePoints(__instance.GetComponentInParent<SubRoot>());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);

            // PatchTranspiler(harmony, TARGET_METHOD);
        }

        /*
        public static readonly OpCode INJECTION_OPCODE = OpCodes.Stloc_0;

        /// <summary>
        /// When random number seeds are synced, we'll need to inject code after the index is generated. This injects just after the first line of code.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                
                if (instruction.opcode.Equals(INJECTION_OPCODE))
                {
                    //Multiplayer.Logic.Cyclops.ToggleExternalDamagePoint(gameObject, true)

                    yield return new ValidatedCodeInstruction(OpCodes.Ldsfld, typeof(Multiplayer).GetField("Logic", BindingFlags.Static | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Logic).GetMethod("get_Cyclops", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldarg_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldfld, TARGET_CLASS.GetField("unusedDamagePoints", BindingFlags.Instance | BindingFlags.NonPublic));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(List<CyclopsDamagePoint>).GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public));
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(CyclopsDamagePoint).GetMethod("get_gameObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static));
                    yield return new ValidatedCodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new ValidatedCodeInstruction(OpCodes.Callvirt, typeof(Cyclops).GetMethod("ToggleExternalDamagePoint", BindingFlags.Public | BindingFlags.Instance));
                }
            }
        }*/
    }
}
