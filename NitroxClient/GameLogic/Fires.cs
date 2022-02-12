using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    /// <summary>
    /// Handles all of the <see cref="Fire"/>s in the game. Currently, the only known Fire spawning is in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>. The
    /// fires in the Aurora come loaded with the map and do not grow in size. If we want to create a Fire spawning mechanic outside of Cyclops fires, it should be
    /// added to <see cref="Fires.Create(string, Optional{string}, Optional{CyclopsRooms}, Optional{int})"/>. Fire dousing goes by Id and does not need to be 
    /// modified
    /// </summary>
    public class Fires
    {
        private readonly IPacketSender packetSender;

        /// <summary>
        /// Used to reduce the <see cref="FireDoused"/> packet spam as fires are being doused. A packet is only sent after
        /// the douse amount surpasses <see cref="FIRE_DOUSE_AMOUNT_TRIGGER"/>
        /// </summary>
        private readonly Dictionary<NitroxId, float> fireDouseAmount = new Dictionary<NitroxId, float>();

        /// <summary>
        /// Each extinguisher hit is from 0.15 to 0.25. 5 is a bit less than half a second of full extinguishing
        /// </summary>
        private const float FIRE_DOUSE_AMOUNT_TRIGGER = 5f;

        public Fires(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        /// <summary>
        /// Triggered when <see cref="SubFire.CreateFire(SubFire.RoomFire)"/> is executed. To create a new fire manually, 
        /// call <see cref="Create(string, Optional{string}, Optional{CyclopsRooms}, Optional{int})"/>
        /// </summary>
        public void OnCreate(Fire fire, SubFire.RoomFire room, int nodeIndex)
        {
            NitroxId subRootId = NitroxEntity.GetId(fire.fireSubRoot.gameObject);

            CyclopsFireCreated packet = new CyclopsFireCreated(NitroxEntity.GetId(fire.gameObject), subRootId, room.roomLinks.room, nodeIndex);
            packetSender.Send(packet);
        }

        /// <summary>
        /// Triggered when <see cref="Fire.Douse(float)"/> is executed. To Douse a fire manually, retrieve the <see cref="Fire"/> call the Douse method
        /// </summary>
        public void OnDouse(Fire fire, float douseAmount)
        {
            NitroxId fireId = NitroxEntity.GetId(fire.gameObject);

            // Temporary packet limiter
            if (!fireDouseAmount.ContainsKey(fireId))
            {
                fireDouseAmount.Add(fireId, douseAmount);
            }
            else
            {
                float summedDouseAmount = fireDouseAmount[fireId] + douseAmount;

                if (summedDouseAmount > FIRE_DOUSE_AMOUNT_TRIGGER)
                {
                    // It is significantly faster to keep the key as a 0 value than to remove it and re-add it later.
                    fireDouseAmount[fireId] = 0;

                    FireDoused packet = new FireDoused(fireId, douseAmount);
                    packetSender.Send(packet);
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="Fire"/>. Majority of code copied from <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>. Currently does not support Fires created outside of a Cyclops
        /// </summary>
        /// <param name="fireGuid">Id of the Fire. Used for identification when dousing the fire</param>
        /// <param name="subRootGuid">Id of the Cyclops <see cref="SubRoot"/></param>
        /// <param name="room">The room the Fire will be spawned in</param>
        /// <param name="spawnNodeIndex">Each <see cref="CyclopsRooms"/> has multiple static Fire spawn points called spawnNodes. If the wrong index is provided,
        ///     the clients will see fires in different places from the owner</param>
        public void Create(CyclopsFireData fireData)
        {
            SubFire subFire = NitroxEntity.RequireObjectFrom(fireData.CyclopsId).GetComponent<SubRoot>().damageManager.subFire;
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = subFire.roomFires;
            // Copied from SubFire_CreateFire_Patch, which copies from SubFire.CreateFire()
            Transform transform2 = roomFiresDict[fireData.Room].spawnNodes[fireData.NodeIndex];

            // If a fire already exists at the node, replace the old Id with the new one
            if (transform2.childCount > 0)
            {
                Fire existingFire = transform2.GetComponentInChildren<Fire>();

                if (NitroxEntity.GetId(existingFire.gameObject) != fireData.CyclopsId)
                {
                    Log.Error("[Fires.Create Fire already exists at node index " + fireData.NodeIndex
                        + "! Replacing existing Fire Id " + NitroxEntity.GetId(existingFire.gameObject)
                        + " with Id " + fireData.CyclopsId
                        + "]");

                    NitroxEntity.SetNewId(existingFire.gameObject, fireData.CyclopsId);
                }

                return;
            }

            List<Transform> availableNodes = subFire.availableNodes;
            availableNodes.Clear();
            foreach (Transform transform in roomFiresDict[fireData.Room].spawnNodes)
            {
                if (transform.childCount == 0)
                {
                    availableNodes.Add(transform);
                }
            }
            roomFiresDict[fireData.Room].fireValue++;
            PrefabSpawn component = transform2.GetComponent<PrefabSpawn>();
            if (component == null)
            {
                return;
            }
            else
            {
                Log.Error("[FireCreatedProcessor Cannot create new Cyclops fire! PrefabSpawn component could not be found in fire node!"
                    + " Fire Id: " + fireData.FireId
                    + " SubRoot Id: " + fireData.CyclopsId
                    + " Room: " + fireData.Room
                    + " NodeIndex: " + fireData.NodeIndex
                    + "]");
            }
            GameObject gameObject = component.SpawnManual();
            Fire componentInChildren = gameObject.GetComponentInChildren<Fire>();
            if (componentInChildren)
            {
                componentInChildren.fireSubRoot = subFire.subRoot;
                NitroxEntity.SetNewId(componentInChildren.gameObject, fireData.FireId);
            }

            subFire.roomFires = roomFiresDict;
            subFire.availableNodes = availableNodes;
        }
    }
}
