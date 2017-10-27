﻿using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateShield : AuthenticatedPacket
    {
        public String Guid { get; }

        public CyclopsActivateShield(String playerId, String guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
