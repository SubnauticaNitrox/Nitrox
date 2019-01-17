using System;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [Serializable]
    public class FireCreated : Packet
    {
        public string Guid { get; }
        public Optional<string> CyclopsGuid { get; }
        public Optional<CyclopsRooms> Room { get; }

        public FireCreated(string guid, Optional<string> cyclopsGuid, Optional<CyclopsRooms> room)
        {
            Guid = guid;
            CyclopsGuid = cyclopsGuid;
            Room = room;
        }
    }
}
