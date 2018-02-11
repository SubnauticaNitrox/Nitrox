using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
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

            DouseFire(cyclops.GetComponent<SubFire>(), packet.Room, packet.FireTransformIndex, packet.FireIndex, packet.DouseAmount);
        }

        /// <summary>
        /// Executes the logic in <see cref="Fire.Douse(float)"/> while not calling the method itself to avoid endless looped calls. 
        /// If the fire is extinguished (no health left), it will pass a large float to trigger the private <see cref="Fire.Extinguish()"/> method.
        /// </summary>
        /// <param name="room">The room the fire is in. Generally passed via <see cref="CyclopsDamage"/> packet.</param>
        /// <param name="fire">The fire to douse. Generally passed via <see cref="CyclopsDamage"/> packet. Passing a large float will extinguish the fire.</param>
        private void DouseFire(SubFire fireManager, CyclopsRooms room, int fireTransformIndex, int fireIndex, float douseAmount)
        {
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)fireManager.ReflectionGet("roomFires");

            // What a beautiful monster this is.
            if (roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?.Length <= fireIndex)
            {
                Log.Error("[Cyclops.DouseFire fireIndex larger than number of Fires fireIndex: " + fireIndex.ToString()
                    + " Fire Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            Fire fire = roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?[fireIndex];
            if (fire == null)
            {
                Log.Error("[Cyclops.DouseFire could not pull Fire object at index: " + fireIndex.ToString()
                    + " Fire Index Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            using (packetSender.Suppress<CyclopsFireHealthChanged>())
            {
                fire.Douse(douseAmount);
            }
        }
    }
}
