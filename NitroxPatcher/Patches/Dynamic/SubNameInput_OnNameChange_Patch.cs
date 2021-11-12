using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.Unity.Helper;

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
                NitroxId controllerId = null;
                Vehicle vehicle = subname.GetComponentInParent<Vehicle>();
                SubRoot subRoot = subname.GetComponentInParent<SubRoot>();
                Rocket rocket = subname.GetComponentInParent<Rocket>();

                if (vehicle)
                {
                    parentVehicle = vehicle.gameObject;

                    GameObject baseCell = __instance.gameObject.RequireComponentInParent<BaseCell>().gameObject;
                    GameObject moonpool = baseCell.RequireComponentInChildren<BaseFoundationPiece>().gameObject;

                    controllerId = NitroxEntity.GetId(moonpool);
                }
                else if (subRoot)
                {
                    parentVehicle = subRoot.gameObject;
                }
                else
                {
                    parentVehicle = rocket.gameObject;
                }

                NitroxId vehicleId = NitroxEntity.GetId(parentVehicle);
                VehicleNameChange packet = new(controllerId, vehicleId, subname.GetName());
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
