using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnColorChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
        {
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (subname != null)
            {
                GameObject vehicle;
                vehicle = subname.GetComponent<Vehicle>().gameObject;
                string guid = GuidHelper.GetGuid(vehicle);
                VehicleColorChange packet = new VehicleColorChange(__instance.SelectedColorIndex, guid, eventData.hsb, eventData.color);
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
