using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateHorn : AuthenticatedPacket
    {
        public String Guid { get; private set; }

        public CyclopsActivateHorn(String playerId, String guid) : base(playerId)
        {
            this.Guid = guid;
        }
        
        public override string ToString()
        {
            return "[CyclopsActivateHorn PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
