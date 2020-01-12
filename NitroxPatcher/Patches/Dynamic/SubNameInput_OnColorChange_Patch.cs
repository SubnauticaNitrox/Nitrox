using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnColorChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
        {
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (subname != null)
            {
                GameObject parentVehicle;
                Vehicle vehicle = subname.GetComponentInParent<Vehicle>();
                SubRoot subRoot = subname.GetComponentInParent<SubRoot>();
                if (vehicle)
                {
                    parentVehicle = vehicle.gameObject;
                }
                else
                {
                    parentVehicle = subRoot.gameObject;
                }

                NitroxId id = NitroxIdentifier.GetId(parentVehicle);
                VehicleColorChange packet = new VehicleColorChange(__instance.SelectedColorIndex, id, eventData.hsb, eventData.color);
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
