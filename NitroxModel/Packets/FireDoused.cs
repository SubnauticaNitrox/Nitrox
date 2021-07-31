using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing
    /// </summary>
    [Serializable]
    public class FireDoused : Packet
    {
        public NitroxId Id { get; }
        public float DouseAmount { get; }

        /// <param name="id">The Fire id</param>
        /// <param name="douseAmount">The amount to douse the fire by. A large number will extinguish the fire. A large number still calls the same 
        ///     method, <see cref="Fire.Douse(float)"/>, which will call <see cref="Fire.Extinguish"/> if the douse amount would extinguish it.</param>
        public FireDoused(NitroxId id, float douseAmount)
        {
            Id = id;
            DouseAmount = douseAmount;
        }
    }
}
