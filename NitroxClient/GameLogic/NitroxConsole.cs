using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class NitroxConsole
    {
        public static bool DisableConsole = true;

        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;
        private readonly Item item;

        public NitroxConsole(IPacketSender packetSender, Vehicles vehicles, Item item)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.item = item;
        }

        //List of things that can be spawned : https://subnauticacommands.com/items
        public void Spawn(GameObject gameObject)
        {
            TechType techType = CraftData.GetTechType(gameObject);

            try
            {
                if (VehicleHelper.IsVehicle(techType))
                {
                    SpawnVehicle(gameObject);
                }
                else
                {
                    SpawnItem(gameObject);
                    //TODO: Add support for no AI creature that need to be spawned as well
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

            Log.Debug($"Spawning vehicle {vehicleModel.TechType} with id {vehicleModel.Id} at {vehicleModel.Position}");
            packetSender.Send(vehicleSpawned);

            vehicles.SpawnDefaultBatteries(vehicleModel);
        }

        /// <summary>
        /// Spawns a Pickupable item
        /// </summary>
        private void SpawnItem(GameObject gameObject)
        {
            Optional<Pickupable> opitem = Optional.OfNullable(gameObject.GetComponent<Pickupable>());

            if (opitem.HasValue)
            {
                Log.Debug($"Spawning item {opitem.Value.GetTechName()} at {gameObject.transform.position}");
                item.Dropped(gameObject, opitem.Value.GetTechType(), gameObject.transform.position);
            }
        }
    }
}
