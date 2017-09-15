using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using System;
using System.Reflection;
using NitroxClient;

namespace NitroxPatcher.Patches
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnDeselect", BindingFlags.Public | BindingFlags.Instance);
        public static readonly MethodInfo TARGET_METHOD2 = TARGET_CLASS.GetMethod("SetSelected", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance)
        {
            DoFix(__instance);
        }

        public static void Prefix(SubNameInput __instance)
        {
            DoFix(__instance);
        }

        public static void DoFix(SubNameInput __instance)
        {
            int index = __instance.SelectedColorIndex;
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (Player.main.GetCurrentSub() != null && subname != null)
            {
                SubNameInput.ColorData[] colorData = __instance.colorData;
                if (colorData != null && index < colorData.Length)
                {
                    Color colorDataColor = ApiHelper.Color(colorData[index].image.color);
                    Vector3 colorDataHSB = ApiHelper.Vector3(subname.GetColor(index));
                    String guid = GuidHelper.GetGuid(Player.main.GetCurrentSub().gameObject);
                    Multiplayer.Logic.Cyclops.ChangeColor(guid, __instance.SelectedColorIndex, colorDataHSB, colorDataColor);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
            this.PatchPrefix(harmony, TARGET_METHOD2);
        }
    }
}
