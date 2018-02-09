using System;

namespace NitroxModel.Packets
{
    /// <summary>
    /// <para>
    /// Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing, although that may change in the future.
    /// </para>
    /// <para>
    /// Fires are triple embedded. It goes from <see cref="SubFire.roomFires"/>, which is a private instance of <see cref="Dictionary{CyclopsRooms, SubFire.RoomFire}"/>. 
    /// Inside, there's <see cref="SubFire.RoomFire.spawnNodes"/>, which are a list of <see cref="UnityEngine.Transform"/>. Each <see cref="UnityEngine.Transform"/> 
    /// is a location inside a <see cref="CyclopsRooms"/> that can contain multiple <see cref="Fire"/>s as children. In order to find a fire,
    /// we need the <see cref="CyclopsRooms"/> it is in, the <see cref="SubFire.RoomFire.spawnNodes"/> index, and finally the 
    /// index of the fire inside the <see cref="UnityEngine.Transform"/>.
    /// </para>
    /// </summary>
    [Serializable]
    public class CyclopsFireHealthChanged : Packet
    {
        public string Guid { get; }
        public CyclopsRooms Room { get; }
        public int FireTransformIndex { get; }
        public int FireIndex { get; }
        public float DouseAmount { get; }

        /// <param name="guid">The Cyclops guid</param>
        /// <param name="room">The <see cref="SubFire.RoomFire"/> the fire is in</param>
        /// <param name="fireNodeIndex">The <see cref="SubFire.RoomFire.spawnNodes"/> index the fire is in.</param>
        /// <param name="fireIndex">The index of the <see cref="Fire"/> located in the <see cref="UnityEngine.Transform"/> 
        ///     found in <see cref="SubFire.RoomFire.spawnNodes"/>.</param>
        /// <param name="douseAmount">The amount to douse the fire by. A large number will extinguish the fire. A large number still calls the same 
        ///     method, <see cref="Fire.Douse(float)"/>, which will call <see cref="Fire.Extinguish"/> if the douse amount would extinguish it.</param>
        public CyclopsFireHealthChanged(string guid, CyclopsRooms room, int fireNodeIndex, int fireIndex, float douseAmount)
        {
            Guid = guid;
            Room = room;
            FireTransformIndex = fireNodeIndex;
            FireIndex = fireIndex;
            DouseAmount = douseAmount;
        }
    }
}
