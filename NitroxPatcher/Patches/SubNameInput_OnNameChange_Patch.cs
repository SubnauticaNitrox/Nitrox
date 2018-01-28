using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
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
                GameObject parentVehicle;

                // This patch works for the vehicles as well as the cyclops; this has to be found for a proper and synced guid.
                SubRoot subRoot = subname.GetComponentInParent<SubRoot>();
                if (subRoot)
                {
                    parentVehicle = subRoot.gameObject;
                }
                else
                {
                    parentVehicle = subname.GetComponent<Vehicle>().gameObject;
                }

                string guid = GuidHelper.GetGuid(parentVehicle);
                VehicleNameChange packet = new VehicleNameChange(guid, subname.GetName());
                Multiplayer.PacketSender.send(packet);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
