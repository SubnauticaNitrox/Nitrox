using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PowerLevelChanged : Packet
    {
        public string Guid { get; }
        public float Amount { get; }
        public PowerType PowerType { get; }

        public PowerLevelChanged(string guid, float amount, PowerType powerType)
        {
            Guid = guid;
            Amount = amount;
            PowerType = powerType;
        }

        public override string ToString()
        {
            return "[PowerLevelChanged - Amount: " + Amount + " PowerType: " + PowerType + " guid: " + Guid + "]";
        }
    }
}
