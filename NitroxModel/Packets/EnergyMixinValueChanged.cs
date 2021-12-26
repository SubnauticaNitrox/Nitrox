using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EnergyMixinValueChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId OwnerId { get; protected set; }
        [Index(1)]
        public virtual float Value { get; protected set; }
        [Index(2)]
        public virtual ItemData BatteryData { get; protected set; }

        private EnergyMixinValueChanged() { }

        public EnergyMixinValueChanged(NitroxId ownerId, float value, ItemData batteryData)
        {
            OwnerId = ownerId;
            Value = value;
            BatteryData = batteryData;
        }
    }
}
