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
using NitroxClient.Unity.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SubNameInput_OnColorChange_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnColorChange(default(ColorChangeEventData)));

        public static void Postfix(SubNameInput __instance, ColorChangeEventData eventData)
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
                } else if (subRoot)
                {
                    parentVehicle = subRoot.gameObject;
                } else
                {
                    parentVehicle = rocket.gameObject;
                }

                NitroxId vehicleId = NitroxEntity.GetId(parentVehicle);
                VehicleColorChange packet = new(__instance.SelectedColorIndex, controllerId, vehicleId, eventData.hsb.ToDto(), eventData.color.ToDto());
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
