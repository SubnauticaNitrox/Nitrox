using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class NitroxConsole
    {
        public static bool DisableConsole = true;

        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public NitroxConsole(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        //List of things that can be spawned : https://subnauticacommands.com/items
        public void Spawn(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);

            try
            {

                if (vehicles.IsVehicle(techType))
                {
                    SpawnVehicle(gameObject);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Error while trying to spawn \"{techType}\" from devconsole", ex);
            }
        }

        /// <summary>
        /// Spawns a Seamoth or an Exosuit
        /// </summary>
        private void SpawnVehicle(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);
            VehicleModel vehicleModel = vehicles.BuildVehicleModelFrom(gameObject, techType);
            Validate.NotNull(vehicleModel, $"Unable to sync spawned vehicle ({vehicleModel.TechType} - {vehicleModel.Id}) from devconsole");

            VehicleSpawned vehicleSpawned = new VehicleSpawned(SerializationHelper.GetBytes(gameObject), vehicleModel);
            vehicles.AddVehicle(vehicleModel);

            packetSender.Send(vehicleSpawned);

            vehicles.SpawnDefaultBatteries(vehicleModel);
        }

        /// <summary>
        /// Spawns a creature
        /// </summary>
        private void SpawnCreature(GameObject gameObject)
        {
            Optional<Creature> opcreature = Optional.OfNullable(gameObject.GetComponent<Creature>());

            if (opcreature.HasValue)
            {
                Log.Debug($"NEED TO SPAWN A CREATURE HERE {opcreature.Value.name}");
                throw new NotImplementedException();
            }
        }

        private void SpawnItem(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        private void SpawnBlueprint(GameObject gameObject)
        {
            throw new NotImplementedException();
        }
    }
}
