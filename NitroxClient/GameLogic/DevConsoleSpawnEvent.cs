using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
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
        /// List of things that can be spawned : https://subnauticacommands.com/items
        /// Vehicle that can be spawn : Seamoth, Exosuit
        /// </summary>
        public void Spawn(GameObject gameObject)
        {
            try
            {
                List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractInteractiveChildren(gameObject);
                TechType techtype = CraftData.GetTechType(gameObject);

                ConstructorBeginCrafting constructorBeginCrafting = VehicleHelper.BuildFrom(gameObject, techtype, childIdentifiers, new NitroxId(), 0f);
                VehicleModel vehicleModel = VehicleModelFactory.BuildFrom(constructorBeginCrafting);

                if (vehicleModel != null)
                {
                    VehicleSpawned vehicleSpawned = new VehicleSpawned(SerializationHelper.GetBytes(gameObject), vehicleModel);
                    vehicles.AddVehicle(vehicleModel);

                    packetSender.Send(vehicleSpawned);

                    VehicleHelper.SpawnDefaultBatteries(gameObject, childIdentifiers);
                }
                else
                {
                    Log.Error("Unable to sync spawned vehicle from devconsole");
                }
            } catch (Exception ex)
            {
                Log.Error("Error while trying to spawn from devconsole", ex);
            }
        }
    }
}
