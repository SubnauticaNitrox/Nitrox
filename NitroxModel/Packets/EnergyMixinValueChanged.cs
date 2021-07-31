using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EnergyMixinValueChanged : Packet
    {
        public NitroxId OwnerId { get; }
        public float Value { get; }
        public ItemData BatteryData { get; }

        public EnergyMixinValueChanged(NitroxId ownerId, float value, ItemData batteryData)
        {
            OwnerId = ownerId;
            Value = value;
            BatteryData = batteryData;
        }
    }
}
