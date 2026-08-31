using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    /// <summary>
    /// Triggered when a fire has been doused. Fire growth is a static thing, so we only need to track dousing
    /// </summary>
    [Serializable]
    public class FireDoused : Packet
    {
        public NitroxId Id { get; }
        public float Health { get; }
        public bool Extinguished { get; }

        /// <param name="id">The Fire id</param>
        /// <param name="health">The new health of the fire</param>
        /// <param name="extinguished">Whether the fire was completely extinguished</param>
        public FireDoused(NitroxId id, float health, bool extinguished)
        {
            Id = id;
            Health = health;
            Extinguished = extinguished;
        }
    }
}
