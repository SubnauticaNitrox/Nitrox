using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class MobileVehicleBay
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public MobileVehicleBay(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public void BeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            NitroxId constructorId = NitroxEntity.GetId(constructor);

            Log.Debug("Building item from constructor with id: " + constructorId);

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.HasValue)
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Value;
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(constructedObject);

                VehicleModel vehicleModel = vehicles.BuildVehicleModelFrom(constructedObject, techType);
                vehicles.AddVehicle(vehicleModel);

                packetSender.Send(new ConstructorBeginCrafting(vehicleModel, constructorId, duration));

                vehicles.SpawnDefaultBatteries(constructedObject, childIdentifiers);

                MonoBehaviour monoBehaviour = constructor.GetComponent<MonoBehaviour>();
                //We want to store the fallen position of the object to avoid flying object on reload 
                if (monoBehaviour)
                {
                    monoBehaviour.StartCoroutine(vehicles.UpdateVehiclePositionAfterSpawn(vehicleModel, constructedObject, duration + 10.0f));
                }
            }
            else
            {
                Log.Error("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }
    }
}
