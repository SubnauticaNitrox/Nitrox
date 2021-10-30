using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SubNameInput_OnNameChange_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnNameChange(default(string)));

        public static void Postfix(SubNameInput __instance)
        {
            SubName subname = __instance.target;
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
                VehicleNameChange packet = new(id, subname.GetName());
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
