using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [ZeroFormattable]
    public class CyclopsFireCreated : Packet
    {
        [Index(0)]
        public virtual CyclopsFireData FireCreatedData { get; protected set; }

        private CyclopsFireCreated() { }

        public CyclopsFireCreated(NitroxId id, NitroxId cyclopsId, CyclopsRooms room, int nodeIndex)
        {
            FireCreatedData = new CyclopsFireData(id, cyclopsId, room, nodeIndex);
        }

        public override string ToString()
        {
            return $"[CyclopsFireCreated - {FireCreatedData}]";
        }
    }
}
