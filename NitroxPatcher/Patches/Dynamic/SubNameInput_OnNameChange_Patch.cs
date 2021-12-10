using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
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
            if (subname)
            {
                GameObject parentVehicle;
                NitroxId controllerId = null;

                if (subname.TryGetComponent(out Vehicle vehicle))
                {
                    GameObject baseCell = __instance.gameObject.RequireComponentInParent<BaseCell>().gameObject;
                    GameObject moonpool = baseCell.RequireComponentInChildren<BaseFoundationPiece>().gameObject;

                    controllerId = NitroxEntity.GetId(moonpool);
                    parentVehicle = vehicle.gameObject;
                }
                else if (subname.TryGetComponentInParent(out SubRoot subRoot))
                {
                    parentVehicle = subRoot.gameObject;
                }
                else if (subname.TryGetComponentInParent(out Rocket rocket))
                {
                    // For some reason only the rocket has a full functioning ghost with a different NitroxId when spawning/constructing, so we are ignoring it.
                    if (rocket.TryGetComponentInChildren(out VFXConstructing constructing) && !constructing.isDone)
                    {
                        return;
                    }

                    parentVehicle = rocket.gameObject;
                }
                else
                {
                    Log.Error($"[SubNameInput_OnNameChange_Patch] The GameObject {subname.gameObject.name} doesn't have a Vehicle/SubRoot/Rocket component.");
                    return;
                }

                NitroxId vehicleId = NitroxEntity.GetId(parentVehicle);
                VehicleNameChange packet = new(controllerId, vehicleId, subname.GetName());
                Resolve<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
