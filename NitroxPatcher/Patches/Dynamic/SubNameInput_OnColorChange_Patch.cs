using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(SubNameInput).GetMethod("OnColorChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
        {
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (subname != null)
            {
                GameObject parentVehicle;
                Vehicle vehicle = subname.GetComponentInParent<Vehicle>();
                SubRoot subRoot = subname.GetComponentInParent<SubRoot>();
                Rocket rocket = subname.GetComponentInParent<Rocket>();

                if (vehicle)
                {
                    parentVehicle = vehicle.gameObject;
                }
                else if (rocket)
                {
                    parentVehicle = rocket.gameObject;
                }
                else
                {
                    parentVehicle = subRoot.gameObject;
                }

                NitroxId id = NitroxEntity.GetId(parentVehicle);
                VehicleColorChange packet = new VehicleColorChange(__instance.SelectedColorIndex, id, eventData.hsb.ToDto(), eventData.color.ToDto());
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
