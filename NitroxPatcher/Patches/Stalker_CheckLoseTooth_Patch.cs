using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Stalker_CheckLoseTooth_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Stalker);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CheckLoseTooth", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(Stalker __instance, GameObject target)
        {
            float hardness = 0f;
            TechType techType = CraftData.GetTechType(target);
            LiveMixin liveMixin = target.GetComponent<LiveMixin>();
            
            if (techType == TechType.ScrapMetal || techType == TechType.Titanium
             || techType == TechType.Seamoth || techType == TechType.Exosuit)
            {
                hardness = 0.3f; //15% probability
            }
            else if(liveMixin != null && liveMixin.IsAlive())
            {
                hardness = 0.1f; //5% probability
            }

            if (UnityEngine.Random.value < hardness && UnityEngine.Random.value < 0.5f)
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
