using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
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
            return $"[EnergyMixinValueChanged: Id: {Id}, Value: {Value}";
        }
    }
}
