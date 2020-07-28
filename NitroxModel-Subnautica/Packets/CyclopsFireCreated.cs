using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [Serializable]
    public class CyclopsFireCreated : Packet
    {
        public CyclopsFireData FireCreatedData { get; }

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
