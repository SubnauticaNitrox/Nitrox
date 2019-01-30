using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [Serializable]
    public class FireCreated : Packet
    {
        public CyclopsFireData FireCreatedData { get; }

        public FireCreated(string guid, string cyclopsGuid, CyclopsRooms room, int nodeIndex)
        {
            FireCreatedData = new CyclopsFireData(guid, cyclopsGuid, room, nodeIndex);
        }

        public override string ToString()
        {
            return "[FireCreated " + FireCreatedData + "]";
        }
    }
}
