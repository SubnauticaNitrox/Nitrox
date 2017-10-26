using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleFloodLights : AuthenticatedPacket
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public CyclopsToggleFloodLights(string playerId, string guid, bool isOn) : base(playerId)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleFloodLights PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
