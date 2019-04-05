using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SpawnConsoleCommandEvent : Packet
    {
        public byte[] SerializeData;
        public VehicleModel Vehiclemodel;

        public SpawnConsoleCommandEvent(byte[] serializeData, VehicleModel vehiclemodel)
        {
            SerializeData = serializeData;
            Vehiclemodel = vehiclemodel;
        }


    }
}
