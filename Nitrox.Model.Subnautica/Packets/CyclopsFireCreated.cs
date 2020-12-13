using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets
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
