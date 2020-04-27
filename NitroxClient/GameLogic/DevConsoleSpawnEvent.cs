using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class DevConsoleSpawnEvent
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public DevConsoleSpawnEvent(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        /// <summary>
        /// Only Seamoth and exosuit can be spawned with /spawn command
        /// Cyclops is spawned throught sub cyclops
        /// </summary>
        public void Spawn(GameObject gameObject)
        {
            List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(gameObject);
            Optional<Vehicle> opvehicle = Optional.OfNullable(gameObject.GetComponent<Vehicle>());
            NitroxId constructedObjectId = NitroxEntity.GetId(gameObject);
            string name = string.Empty;
            float health = 200f;
            Vector3[] Colours;

            if (opvehicle.HasValue)
            {
                Optional<LiveMixin> livemixin = Optional.OfNullable(opvehicle.Value.GetComponent<LiveMixin>());

                if (livemixin.HasValue)
                {
                    health = livemixin.Value.health;
                }

                name = opvehicle.Value.GetName();
                Colours = opvehicle.Value.subName?.GetColors();
            }
            else
            {
                Colours = new Vector3[] { new Vector3(0f, 0f, 1f) };
            }

            ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(
                new NitroxId(),
                NitroxEntity.GetId(gameObject),
                CraftData.GetTechType(gameObject).Model(),
                0,
                childIdentifiers,
                gameObject.transform.position,
                gameObject.transform.rotation,
                name,
                Colours,
                Colours,
                health
            );

            VehicleModel vehicleModel = VehicleModelFactory.BuildFrom(beginCrafting);
            VehicleSpawned vehicleSpawned = new VehicleSpawned(SerializationHelper.GetBytes(gameObject), vehicleModel);
            vehicles.AddVehicle(vehicleModel);

            SpawnDefaultBatteries(gameObject, childIdentifiers);

            packetSender.Send(vehicleSpawned);
        }

        private void SpawnDefaultBatteries(GameObject constructedObject, List<InteractiveChildObjectIdentifier> childIdentifiers)
        {

            Optional<EnergyMixin> opEnergy = Optional.OfNullable(constructedObject.GetComponent<EnergyMixin>());
            if (opEnergy.HasValue)
            {
                EnergyMixin mixin = opEnergy.Value;
                mixin.ReflectionSet("allowedToPlaySounds", false);
                mixin.SetBattery(mixin.defaultBattery, 1);
                mixin.ReflectionSet("allowedToPlaySounds", true);
            }

            foreach (InteractiveChildObjectIdentifier identifier in childIdentifiers)
            {
                Optional<GameObject> opChildGameObject = NitroxEntity.GetObjectFrom(identifier.Id);

                if (opChildGameObject.HasValue)
                {
                    Optional<EnergyMixin> opEnergyMixin = Optional.OfNullable(opChildGameObject.Value.GetComponent<EnergyMixin>());

                    if (opEnergyMixin.HasValue)
                    {

                        EnergyMixin mixin = opEnergyMixin.Value;
                        mixin.ReflectionSet("allowedToPlaySounds", false);
                        mixin.SetBattery(mixin.defaultBattery, 1);
                        mixin.ReflectionSet("allowedToPlaySounds", true);
                    }
                }
            }
        }
    }
}
