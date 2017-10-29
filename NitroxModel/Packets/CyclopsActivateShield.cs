using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsActivateShield : AuthenticatedPacket
    {
        public string Guid { get; }

        public CyclopsActivateShield(string playerId, string guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
