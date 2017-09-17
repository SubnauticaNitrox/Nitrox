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
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnColorChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
        {
            Player player = (Player)__instance.ReflectionGet("player");
            if (player != null)
            {
                Color color = ApiHelper.Color(eventData.color);
                Vector3 hsb = ApiHelper.Vector3(eventData.hsb);
                String guid = GuidHelper.GetGuid(player.GetCurrentSub().gameObject);
                Multiplayer.Logic.Cyclops.ChangeColor(guid, __instance.SelectedColorIndex, hsb, color);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
