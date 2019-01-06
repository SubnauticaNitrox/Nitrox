using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Stalker_CheckLoseTooth_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Stalker);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CheckLoseTooth", BindingFlags.NonPublic | BindingFlags.Instance);

        //GetComponent<HardnessMixin> was returning null for everything instead of a HardnessMixin with a hardness value. Since this component 
        //isn't used for anything else than the stalker teeth drop, we hard-code the values and bingo.
        public static bool Prefix(Stalker __instance, GameObject target)
        {
            float dropProbability = 0f;
            TechType techType = CraftData.GetTechType(target);
            
            if (techType == TechType.ScrapMetal)
            {
                dropProbability = 0.15f; //15% probability
            }

            if (UnityEngine.Random.value < dropProbability)
            {
                __instance.ReflectionCall("LoseTooth");
            }
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
