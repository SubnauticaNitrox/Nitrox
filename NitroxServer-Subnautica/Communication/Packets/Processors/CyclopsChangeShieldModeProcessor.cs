﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica.Communication.Packets.Processors
{
    class CyclopsChangeShieldModeProcessor : AuthenticatedPacketProcessor<CyclopsChangeShieldMode>
    {
        public VehicleData Vehicles { get; }
        public PlayerManager PlayerManager { get; }

        public CyclopsChangeShieldModeProcessor(VehicleData vehicleData, PlayerManager playerManager)
        {
            Vehicles = vehicleData;
            PlayerManager = playerManager;
        }

        public override void Process(CyclopsChangeShieldMode packet, NitroxServer.Player player)
        {
            Optional<CyclopsModel> opCyclops = Vehicles.GetVehicleModel<CyclopsModel>(packet.Id);
            if (opCyclops.IsPresent())
            {
                opCyclops.Get().ShieldOn = packet.IsOn;
            }
            PlayerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
