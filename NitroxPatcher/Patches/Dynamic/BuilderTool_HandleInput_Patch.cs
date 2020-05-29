using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BuilderTool_HandleInput_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BuilderTool);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HandleInput", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(BuilderTool __instance)
        {
            bool _result = true;

            _result = NitroxServiceLocator.LocateService<Building>().BuilderTool_HandleInput_Pre(__instance.gameObject);
            if (_result == false)
            {
                return _result; //skip further checks and exit here
            }

            //Add more checks here later for other classes (e.g. Crafting)

            return _result;
        }

        public static void Postfix(BuilderTool __instance)
        {
            NitroxServiceLocator.LocateService<Building>().BuilderTool_HandleInput_Post(__instance);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
