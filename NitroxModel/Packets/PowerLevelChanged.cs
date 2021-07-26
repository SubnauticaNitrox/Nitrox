using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PowerLevelChanged : Packet
    {
        public NitroxId Id { get; }
        public float Amount { get; }
        public PowerType PowerType { get; }

        public PowerLevelChanged(NitroxId id, float amount, PowerType powerType)
        {
            Id = id;
            Amount = amount;
            PowerType = powerType;
        }

        public override string ToString()
        {
            return "[PowerLevelChanged - Amount: " + Amount + " PowerType: " + PowerType + " id: " + Id + "]";
        }
    }
}
