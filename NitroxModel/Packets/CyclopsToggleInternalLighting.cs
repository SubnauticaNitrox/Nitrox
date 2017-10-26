using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleInternalLighting : AuthenticatedPacket
    {
        public String Guid { get; }
        public bool IsOn { get; }

        public CyclopsToggleInternalLighting(String playerId, String guid, bool isOn) : base(playerId)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleInternalLighting PlayerId: " + PlayerId + " Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
