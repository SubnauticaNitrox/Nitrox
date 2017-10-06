using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NitroxPatcher.Patches.Client
{
    public class Constructable_Deconstruct_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static readonly OpCode CONSTRUCTION_COMPLETE_INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object CONSTRUCTION_COMPLETE_INJECTION_OPERAND = TARGET_CLASS.GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Constructable __instance)
        {
            if (!__instance._constructed && __instance.constructedAmount > 0)
            {
                Multiplayer.Logic.Building.ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }

            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
            {
                Multiplayer.Logic.Building.DeconstructionComplete(__instance.gameObject);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
