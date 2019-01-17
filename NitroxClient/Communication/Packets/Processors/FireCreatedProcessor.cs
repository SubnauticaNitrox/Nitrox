using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FireCreatedProcessor : ClientPacketProcessor<FireCreated>
    {
        private readonly IPacketSender packetSender;

        public FireCreatedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        /// <summary>
        /// Finds and executes <see cref="Fire.Douse(float)"/>. If the fire is extinguished, it will pass a large float to trigger the private
        /// <see cref="Fire.Extinguish()"/> method.
        /// </summary>
        public override void Process(FireCreated packet)
        {
            // A Fire is either on a Cyclops or not. Currently, creating a Fire is only possible if it's in a Cyclops.
            // I have not found any other code that creates a fire
            if (packet.CyclopsGuid.IsPresent() && packet.Room.IsPresent())
            {
                SubFire subFire = GuidHelper.RequireObjectFrom(packet.CyclopsGuid.Get()).GetComponent<SubRoot>().damageManager.subFire;
                Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

                // Copied from SubFire_CreateFire_Patch, which copies from SubFire.CreateFire()
                List<Transform> availableNodes = (List<Transform>)subFire.ReflectionGet("availableNodes");

                // Copied from SubFire.CreateFire(). Need the generated Fire object
                availableNodes.Clear();
                foreach (Transform transform in roomFiresDict[packet.Room.Get()].spawnNodes)
                {
                    if (transform.childCount == 0)
                    {
                        availableNodes.Add(transform);
                    }
                }

                if (availableNodes.Count == 0)
                {
                    return;
                }

                int index = UnityEngine.Random.Range(0, availableNodes.Count);
                Transform transform2 = availableNodes[index];
                roomFiresDict[packet.Room.Get()].fireValue++;
                PrefabSpawn component = transform2.GetComponent<PrefabSpawn>();
                if (component == null)
                {
                    return;
                }
                GameObject gameObject = component.SpawnManual();
                Fire componentInChildren = gameObject.GetComponentInChildren<Fire>();
                if (componentInChildren)
                {
                    componentInChildren.fireSubRoot = subFire.subRoot;
                    GuidHelper.SetNewGuid(componentInChildren.gameObject, packet.Guid);
                }
                
                subFire.ReflectionSet("roomFires", roomFiresDict);
                subFire.ReflectionSet("availableNodes", availableNodes);
            }
            else
            {
                Log.Error("[FireCreatedProcessor No Cyclops Guid or CyclopsRoom passed. There is currently no way to create a Fire outside of a Cyclops! Fire Guid: " + packet.Guid + "]");
            }
        }
    }
}
