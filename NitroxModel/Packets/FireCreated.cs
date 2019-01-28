using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [Serializable]
    public class FireCreated : Packet
    {
        public FireData FireCreatedData { get; }

        public FireCreated(string guid, Optional<string> cyclopsGuid, CyclopsRooms room, int nodeIndex)
        {
            FireCreatedData = new FireData(guid, cyclopsGuid, room, nodeIndex);
        }

        public override string ToString()
        {
            return "[FireCreated " + FireCreatedData.ToString() + "]";
        }
    }
}
