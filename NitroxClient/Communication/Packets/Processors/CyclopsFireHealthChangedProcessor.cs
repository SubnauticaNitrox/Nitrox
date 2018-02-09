using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    /// <summary>
    /// <see cref="Fire"/>'s health can change a lot in a short period of time if multiple clients are extinguishing fires. 
    /// Passing a fully populated <see cref="CyclopsDamage"/> would cause excessive data to be passed.
    /// </summary>
    public class CyclopsFireHealthChangedProcessor : ClientPacketProcessor<CyclopsFireHealthChanged>
    {
        private readonly IPacketSender packetSender;

        public CyclopsFireHealthChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        /// <summary>
        /// Finds and executes <see cref="Fire.Douse(float)"/>. If the fire is extinguished, it will pass a large float to trigger the private
        /// <see cref="Fire.Extinguish()"/> method.
        /// </summary>
        public override void Process(CyclopsFireHealthChanged packet)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(packet.Guid);

            Multiplayer.Logic.Cyclops.DouseFire(cyclops.GetComponent<SubFire>(), packet.Room, packet.FireTransformIndex, packet.FireIndex, packet.DouseAmount);
        }
    }
}
