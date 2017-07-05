using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleMovementProcessor : GenericPacketProcessor<VehicleMovement>
    {
        private VehicleGameObjectManager vehicleGameObjectManager;
        private PlayerGameObjectManager playerGameObjectManager;

        public VehicleMovementProcessor(VehicleGameObjectManager vehicleGameObjectManager, PlayerGameObjectManager playerGameObjectManager)
        {
            this.vehicleGameObjectManager = vehicleGameObjectManager;
            this.playerGameObjectManager = playerGameObjectManager;
        }

        public override void Process(VehicleMovement vehicleMovement)
        {
            playerGameObjectManager.HidePlayerGameObject(vehicleMovement.PlayerId);
            vehicleGameObjectManager.UpdateVehiclePosition(vehicleMovement.Guid, vehicleMovement.TechType, ApiHelper.Vector3(vehicleMovement.PlayerPosition), ApiHelper.Quaternion(vehicleMovement.Rotation));
        }        
    }
}
