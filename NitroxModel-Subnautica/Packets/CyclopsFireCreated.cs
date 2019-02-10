using System;
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

        public CyclopsFireCreated(string guid, string cyclopsGuid, CyclopsRooms room, int nodeIndex)
        {
            FireCreatedData = new CyclopsFireData(guid, cyclopsGuid, room, nodeIndex);
        }

        public override string ToString()
        {
            return "[CyclopsFireCreated " + FireCreatedData + "]";
        }
    }
}
