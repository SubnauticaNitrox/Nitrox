using NitroxModel.DataStructures.GameLogic;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PowerLevelChanged : AuthenticatedPacket
    {
        public String Guid { get; }
        public float Amount { get; }
        public PowerType PowerType { get; }

        public PowerLevelChanged(String playerId, String guid, float amount, PowerType powerType) : base(playerId)
        {
            this.Guid = guid;
            this.Amount = amount;
            this.PowerType = powerType;
        }

        public override string ToString()
        {
            return "[PowerLevelChanged - playerId: " + PlayerId + " Amount: " + Amount + " PowerType: " + PowerType + " guid: " + Guid + "]";
        }
    }
}
