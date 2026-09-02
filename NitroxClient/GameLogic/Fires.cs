using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    /// <summary>
    ///     Handles all of the <see cref="Fire" />s in the game. Currently, the only known Fire spawning is in
    ///     <see cref="SubFire.CreateFire(SubFire.RoomFire)" />. The
    ///     fires in the Aurora come loaded with the map and do not grow in size. If we want to create a Fire spawning mechanic
    ///     outside of Cyclops fires, it should be
    ///     added to <see cref="Fires.Create(CyclopsFireData)" />. Fire dousing goes by Id and does not need to be
    ///     modified
    /// </summary>
    public class Fires
    {
        private readonly Entities entities;
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;

        public Fires(Entities entities, IPacketSender packetSender, ThrottledPacketSender throttledPacketSender)
        {
            this.entities = entities;
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;
        }

        /// <summary>
        ///     Triggered when <see cref="SubFire.CreateFire(SubFire.RoomFire)" /> is executed. To create a new fire manually,
        ///     call <see cref="Create(CyclopsFireData)" />
        /// </summary>
        public void OnCreate(Fire fire, SubFire.RoomFire room, int nodeIndex)
        {
            if (!fire.fireSubRoot.TryGetIdOrWarn(out NitroxId subRootId))
            {
                return;
            }

            NitroxId fireId = NitroxEntity.GenerateNewId(fire.transform.parent.gameObject);

            CyclopsFireCreated packet = new(fireId, subRootId, room.roomLinks.room, nodeIndex);
            packetSender.Send(packet);
        }

        /// <summary>
        ///     Triggered when <see cref="Fire.Douse(float)" /> is executed. To Douse a fire manually, retrieve the
        ///     <see cref="Fire" /> call the Douse method
        /// </summary>
        public void OnDouse(Fire fire, float douseAmount)
        {
            if (!fire.transform.parent.TryGetIdOrWarn(out NitroxId fireId))
            {
                return;
            }

            bool extinguished = !fire.livemixin.IsAlive() || fire.isExtinguished;
            if (extinguished)
            {
                entities.RemoveEntity(fireId);
            }
            FireDoused packet = new(fireId, extinguished ? 0 : fire.livemixin.health);
            throttledPacketSender.SendThrottled(packet, x => x.Id);
        }

        /// <summary>
        ///     Create a new <see cref="Fire" />. Majority of code copied from <see cref="SubFire.CreateFire(SubFire.RoomFire)" />.
        ///     Currently does not support Fires created outside of a Cyclops
        /// </summary>
        public void Create(CyclopsFireData fireData)
        {
            SubFire subFire = NitroxEntity.RequireObjectFrom(fireData.CyclopsId).GetComponent<SubRoot>().damageManager.subFire;
            SubFire.RoomFire roomFire = subFire.roomFires[fireData.Room];
            Transform spawnNode = roomFire.spawnNodes[fireData.NodeIndex];

            // If a fire already exists at the node, replace the old Id with the new one
            if (spawnNode.childCount > 0)
            {
                Transform existingFire = spawnNode.GetComponentInChildren<Fire>().transform.parent;

                if (existingFire.TryGetNitroxId(out NitroxId existingFireId) && existingFireId != fireData.FireId)
                {
                    Log.Warn($"Fire already exists at node index {fireData.NodeIndex}! Replacing existing Fire Id {existingFireId} with Id {fireData.CyclopsId}");

                    NitroxEntity.SetNewId(existingFire.gameObject, fireData.CyclopsId);
                }

                return;
            }

            roomFire.fireValue++;

            PrefabSpawn component = spawnNode.GetComponent<PrefabSpawn>();
            if (!component)
            {
                Log.Error(
                    $"Cannot create new Cyclops fire! PrefabSpawn component could not be found in fire node! Fire Id: {fireData.FireId} SubRoot Id: {fireData.CyclopsId} Room: {fireData.Room} NodeIndex: {fireData.NodeIndex}");
                return;
            }

            component.SpawnManual(gameObject =>
            {
                Fire fire = gameObject.GetComponentInChildren<Fire>();
                if (fire)
                {
                    fire.fireSubRoot = subFire.subRoot;
                    NitroxEntity.SetNewId(fire.transform.parent.gameObject, fireData.FireId);
                }
            });
        }
    }
}
