using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateShield : AuthenticatedPacket
    {
        public String Guid { get; private set; }

        public CyclopsActivateShield(String playerId, String guid) : base(playerId)
        {
            this.Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
