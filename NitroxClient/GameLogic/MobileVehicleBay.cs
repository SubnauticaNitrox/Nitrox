using System;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class MobileVehicleBay
    {
        public static bool TransmitLocalSpawns { get; set; } = true;

        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public MobileVehicleBay(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public void BeginCrafting(ConstructorInput constructor, GameObject constructedObject, TechType techType, float duration)
        {
            if (!TransmitLocalSpawns)
            {
                return;
            }

            // Sometimes build templates, such as the cyclops, are already tagged with IDs.  Remove any that exist to retag.
            UnityEngine.Component.DestroyImmediate(constructedObject.GetComponent<NitroxEntity>());

            NitroxId constructedObjectId = NitroxEntity.GetId(constructedObject);
            NitroxId constructorId = NitroxEntity.GetId(constructor.constructor.gameObject);
            
            VehicleWorldEntity vehicleEntity = new VehicleWorldEntity(constructorId, DayNightCycle.main.timePassedAsFloat, constructedObject.transform.ToDto(), "", false, constructedObjectId, techType.ToDto(), null);
            vehicleEntity.ChildEntities = Entities.GetPrefabChildren(constructedObject, constructedObjectId).ToList();

            packetSender.Send(new EntitySpawnedByClient(vehicleEntity));

            MonoBehaviour monoBehaviour = constructor.GetComponent<MonoBehaviour>();
            //We want to store the fallen position of the object to avoid flying object on reload 
            if (monoBehaviour)
            {
                monoBehaviour.StartCoroutine(vehicles.UpdateVehiclePositionAfterSpawn(constructedObjectId, techType, constructedObject, duration + 10.0f));
            }
        }
    }
}
