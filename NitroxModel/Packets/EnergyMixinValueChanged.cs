using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EnergyMixinValueChanged : Packet
    {
        public NitroxId Id { get; }
        public float Value { get; }
        public ItemData BatteryData { get; }

        public EnergyMixinValueChanged(NitroxId id, float value, ItemData batteryData)
        {
            Id = id;
            Value = value;
            BatteryData = batteryData;
        }

        public override string ToString()
        {
            return $"[EnergyMixinValueChanged: OwnerId: {Id}, Value: {Value}, BatteryId: {BatteryData.ItemId}";
        }
    }
}
