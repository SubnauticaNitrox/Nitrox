using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;
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
            Player player = (Player)__instance.ReflectionGet("player");
            if (player != null)
            {
                String guid = GuidHelper.GetGuid(player.GetCurrentSub().gameObject);
                Multiplayer.Logic.Cyclops.ChangeColor(guid, __instance.SelectedColorIndex, eventData.hsb, eventData.color);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
