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
    public class SubNameInput_OnNameChange_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubNameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnNameChange", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(SubNameInput __instance)
        {
            SubName subname = (SubName)__instance.ReflectionGet("target");
            if (subname != null)
            {
                GameObject vehicle;
                vehicle = subname.GetComponent<Vehicle>().gameObject;
                string guid = GuidHelper.GetGuid(vehicle);
                VehicleNameChange packet = new VehicleNameChange(guid, subname.GetName());
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);

            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
