using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

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

        public void BeginCrafting(ConstructorInput constructor, GameObject constructedObject, TechType techType, float duration)
        {
            NitroxId constructorId = NitroxEntity.GetId(constructor.gameObject);

            Log.Debug($"Building item from constructor with id: {constructorId}");

            VehicleModel vehicleModel = vehicles.BuildVehicleModelFrom(constructedObject, techType);
            vehicles.AddVehicle(vehicleModel);

            packetSender.SendIfGameCode(new ConstructorBeginCrafting(vehicleModel, constructorId, duration));

            MonoBehaviour monoBehaviour = constructor.GetComponent<MonoBehaviour>();
            //We want to store the fallen position of the object to avoid flying object on reload 
            if (monoBehaviour)
            {
                monoBehaviour.StartCoroutine(vehicles.UpdateVehiclePositionAfterSpawn(vehicleModel, constructedObject, duration + 10.0f));
            }
        }
    }
}
