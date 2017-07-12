using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxPatcher.Patches
{
    public class ConstructorInput_Craft_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ConstructorInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Craft", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ConstructorInput __instance, TechType techType, float duration)
        {
            Multiplayer.PacketSender.ConstructorBeginCrafting(__instance.constructor.gameObject, techType, duration);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
