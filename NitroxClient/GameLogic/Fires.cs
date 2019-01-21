using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    /// <summary>
    /// Handles all of the <see cref="Fire"/>s in the game. Currently, the only known Fire spawning is in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>. The
    /// fires in the Aurora come loaded with the map and do not grow in size. If we want to create a Fire spawning mechanic outside of Cyclops fires, it should be
    /// added to <see cref="Fires.CreateFire(string, Optional{string}, Optional{CyclopsRooms}, Optional{int})"/>. Fire dousing goes by Guid and does not need to be 
    /// modified
    /// </summary>
    public class Fires
    {
        private readonly IPacketSender packetSender;

        /// <summary>
        /// Used to reduce the <see cref="FireDoused"/> packet spam as fires are being doused. A packet is only sent after
        /// the douse amount surpasses <see cref="FIRE_DOUSE_AMOUNT_TRIGGER"/>
        /// </summary>
        private readonly Dictionary<string, float> fireDouseAmount = new Dictionary<string, float>();

        /// <summary>
        /// Each extinguisher hit is from 0.15 to 0.25. 5 is a bit less than half a second of full extinguishing
        /// </summary>
        private const float FIRE_DOUSE_AMOUNT_TRIGGER = 5f;

        public Fires(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OnCreate(Fire fire, SubFire.RoomFire room, int nodeIndex)
        {
            Optional<string> subRootGuid = Optional<string>.OfNullable(GuidHelper.GetGuid(fire.fireSubRoot.gameObject));
            Optional<CyclopsRooms> startInRoom = Optional<CyclopsRooms>.OfNullable(room.roomLinks.room);
            Optional<int> activeNodeIndex = Optional<int>.OfNullable(nodeIndex);

            FireCreated packet = new FireCreated(GuidHelper.GetGuid(fire.gameObject), subRootGuid, startInRoom, activeNodeIndex);
            packetSender.Send(packet);
        }

        public void OnDouse(Fire fire, float douseAmount)
        {
            string fireGuid = GuidHelper.GetGuid(fire.gameObject);

            // Temporary packet limiter
            if (!fireDouseAmount.ContainsKey(fireGuid))
            {
                fireDouseAmount.Add(fireGuid, douseAmount);
            }
            else
            {
                float summedDouseAmount = fireDouseAmount[fireGuid] + douseAmount;

                if (summedDouseAmount > FIRE_DOUSE_AMOUNT_TRIGGER)
                {
                    // It is significantly faster to keep the key as a 0 value than to remove it and re-add it later.
                    fireDouseAmount[fireGuid] = 0;

                    FireDoused packet = new FireDoused(fireGuid, douseAmount);
                    packetSender.Send(packet);
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="Fire"/>. Currently does not support Fires created outside of a Cyclops
        /// </summary>
        /// <param name="fireGuid">Guid of the Fire. Used for identification when dousing the fire</param>
        /// <param name="subRootGuid">Guid of the Cyclops <see cref="SubRoot"/></param>
        /// <param name="room">The room the Fire will be spawned in</param>
        /// <param name="spawnNodeIndex">Each <see cref="CyclopsRooms"/> has multiple static Fire spawn points called spawnNodes. If the wrong index is provided,
        ///     the clients will see fires in different places from the owner</param>
        public void CreateFire(string fireGuid, Optional<string> subRootGuid, Optional<CyclopsRooms> room, Optional<int> spawnNodeIndex)
        {
            // A Fire is either on a Cyclops or not. Currently, creating a Fire is only possible if it's in a Cyclops.
            // I have not found any other code that creates a fire
            if (subRootGuid.IsPresent() && room.IsPresent() && spawnNodeIndex.IsPresent())
            {
                SubFire subFire = GuidHelper.RequireObjectFrom(subRootGuid.Get()).GetComponent<SubRoot>().damageManager.subFire;
                Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

                // Copied from SubFire_CreateFire_Patch, which copies from SubFire.CreateFire()
                Transform[] spawnNodes = (Transform[])subFire.ReflectionGet("spawnNodes");

                Transform transform2 = spawnNodes[spawnNodeIndex.Get()];
                roomFiresDict[room.Get()].fireValue++;
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
                    GuidHelper.SetNewGuid(componentInChildren.gameObject, fireGuid);
                }

                subFire.ReflectionSet("roomFires", roomFiresDict);
                subFire.ReflectionSet("availableNodes", spawnNodes);
            }
            else
            {
                // This is where non-Cyclops Fire creation logic should go
                Log.Error("[FireCreatedProcessor No Cyclops Guid, CyclopsRoom, or SpawnNodeIndex passed. There is currently no way to create a Fire outside of a Cyclops! Fire Guid: " + fireGuid + "]");
            }
        }
    }
}
