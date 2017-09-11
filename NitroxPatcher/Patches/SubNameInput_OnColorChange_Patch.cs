using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnColorChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
        {
            if (Player.main.GetCurrentSub() != null)
            {
                Color colorDataColor = new Color(eventData.color.r, eventData.color.g, eventData.color.b, eventData.color.a);
                Vector3 colorDataHSB = new Vector3(eventData.hsb.x, eventData.hsb.y, eventData.hsb.z);
                String guid = GuidHelper.GetGuid(Player.main.GetCurrentSub().gameObject);
                Multiplayer.Logic.Cyclops.ChangeColor(guid, __instance.SelectedColorIndex, colorDataHSB, colorDataColor);
            }
            else
            {
                ErrorMessage.AddMessage("Could not find EventData or CurrentSub from Main-Player to change color");
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
