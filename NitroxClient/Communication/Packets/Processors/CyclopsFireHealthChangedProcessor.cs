using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
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
            SubFire fireManager = cyclops.gameObject.RequireComponent<SubFire>();

            // NitroxServiceLocator.LocateService<Cyclops>().DouseFire(subFire, packet.Room, packet.FireTransformIndex, packet.FireIndex, packet.DouseAmount);

            using (packetSender.Suppress<CyclopsDamage>())
            {
                using (packetSender.Suppress<CyclopsFireHealthChanged>())
                {
                    Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)fireManager.ReflectionGet("roomFires");

                    // What a beautiful monster this is.
                    if (roomFires[packet.Room]?.spawnNodes[packet.FireTransformIndex]?.GetAllComponentsInChildren<Fire>()?.Length <= packet.FireIndex)
                    {
                        Log.Warn("[CyclopsFireHealthChangedProcessor fireIndex larger than number of Fires fireIndex: " + packet.FireIndex.ToString()
                            + " Fire Count: " + roomFires[packet.Room]?.spawnNodes[packet.FireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                            + "]");

                        return;
                    }

                    Fire fire = roomFires[packet.Room]?.spawnNodes[packet.FireTransformIndex]?.GetAllComponentsInChildren<Fire>()?[packet.FireIndex];
                    if (fire == null)
                    {
                        Log.Warn("[CyclopsFireHealthChangedProcessor could not pull Fire object at index: " + packet.FireIndex.ToString()
                            + " Fire Index Count: " + roomFires[packet.Room]?.spawnNodes[packet.FireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                            + "]");

                        return;
                    }

                    fire.Douse(packet.DouseAmount);
                }
            }
        }
    }
}
