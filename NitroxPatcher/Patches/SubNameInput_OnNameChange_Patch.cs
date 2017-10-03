using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class SubNameInput_OnNameChange_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnNameChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance)
        {
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (subname != null && Player.main.GetCurrentSub() != null)
            {
                String guid = GuidHelper.GetGuid(Player.main.GetCurrentSub().gameObject);
                Multiplayer.Logic.Cyclops.ChangeName(guid, subname.GetName());
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
