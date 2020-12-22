using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    /// <summary>
    /// Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing
    /// </summary>
    [Serializable]
    public class FireDoused : Packet
    {
        public NitroxId FireId { get; }
        public float DouseAmount { get; }
        public NitroxId ParentId { get; }
        public NitroxVector3 Position { get; }

        /// <param name="fireId">The Fire id</param>
        /// <param name="douseAmount">The amount to douse the fire by. A large number will extinguish the fire. A large number still calls the same 
        /// <param name="parentId">Nitrox ID of the Fire's parent for fallback in case of fire ID mismatch</param>
        /// <param name="position">Position of the fire within / relative to the parent</param>
        ///     method, <see cref="Fire.Douse(float)"/>, which will call <see cref="Fire.Extinguish"/> if the douse amount would extinguish it.</param>
        public FireDoused(NitroxId fireId, float douseAmount, NitroxId parentId, NitroxVector3 position)
        {
            FireId = fireId;
            DouseAmount = douseAmount;
            ParentId = parentId;
            Position = position;
        }

        public override string ToString()
        {
            return "[FireDoused Id: " + FireId + "Douse Amount: " + DouseAmount + "]";
        }
    }
}
