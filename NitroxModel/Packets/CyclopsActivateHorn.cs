using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateHorn : AuthenticatedPacket
    {
        public string Guid { get; }

        public CyclopsActivateHorn(string playerId, string guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateHorn PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
