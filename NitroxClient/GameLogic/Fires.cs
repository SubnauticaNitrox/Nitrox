using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using System.Security.Cryptography;
using NitroxClient.Communication;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class Fires
    {
        private readonly IPacketSender packetSender;
        private readonly Dictionary<NitroxId, float> fireDouseAmount = new();
        private const float FIRE_DOUSE_AMOUNT_TRIGGER = 5f;

        public Fires(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OnCreate(Fire fire, SubFire.RoomFire room, int nodeIndex)
        {
            if (!fire.TryGetNitroxId(out NitroxId fireId))
            {
                fireId = new NitroxId();
                NitroxEntity.SetNewId(fire.gameObject, fireId);
            }

            if (!fire.fireSubRoot.TryGetIdOrWarn(out NitroxId subRootId))
            {
                OnCreate(fire);
                return;
            }

            packetSender.Send(new CyclopsFireCreated(fireId, subRootId, room.roomLinks.room, nodeIndex));
        }

        public void OnCreate(Fire fire)
        {
            if (PacketSuppressor<FireCreated>.IsSuppressed) return;

            if (!fire.TryGetNitroxId(out NitroxId fireId))
            {
                fireId = GetDeterministicId(fire);
                NitroxEntity.SetNewId(fire.gameObject, fireId);
            }

            if (Multiplayer.Main.InitialSyncCompleted)
            {
                NitroxId parentId = null;
                if (fire.fireSubRoot && fire.fireSubRoot.TryGetNitroxId(out NitroxId subRootId))
                {
                    parentId = subRootId;
                }

                packetSender.Send(new FireCreated(fireId, parentId, fire.transform.position.ToDto(), fire.transform.rotation.ToDto()));
            }
        }

        private NitroxId GetDeterministicId(Fire fire)
        {
            Vector3 pos = fire.transform.position;
            string seed = $"Fire_{pos.x:F1}_{pos.y:F1}_{pos.z:F1}";
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seed));
            return new NitroxId(new Guid(hash));
        }

        public void OnDouse(Fire fire, float douseAmount)
        {
            if (PacketSuppressor<FireDoused>.IsSuppressed) return;
            if (!fire.TryGetIdOrWarn(out NitroxId fireId)) return;

            // Prevent packet spam by accumulating douse amounts iteratively
            fireDouseAmount.TryGetValue(fireId, out float currentAmount);
            float summedDouseAmount = currentAmount + douseAmount;

            if (summedDouseAmount > FIRE_DOUSE_AMOUNT_TRIGGER)
            {
                // Reset the value locally and alert the server of the accumulated douse amount
                fireDouseAmount[fireId] = 0f;
                packetSender.Send(new FireDoused(fireId, summedDouseAmount));
            }
            else
            {
                fireDouseAmount[fireId] = summedDouseAmount;
            }
        }

        public void Create(CyclopsFireData fireData)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(fireData.CyclopsId);
            SubFire subFire = cyclops.GetComponent<SubRoot>().damageManager.subFire;
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = subFire.roomFires;
            Transform transform2 = roomFiresDict[fireData.Room].spawnNodes[fireData.NodeIndex];

            if (transform2.childCount > 0)
            {
                Fire existingFire = transform2.GetComponentInChildren<Fire>();
                if (existingFire.TryGetNitroxId(out NitroxId existingFireId) && existingFireId != fireData.FireId)
                {
                    Log.Error($"[Fires.Create Fire already exists at node index {fireData.NodeIndex}! Replacing existing Fire Id {existingFireId} with Id {fireData.FireId}]");
                    NitroxEntity.SetNewId(existingFire.gameObject, fireData.FireId);
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
            if (!component)
            {
                Log.Error($"[{nameof(CyclopsFireCreatedProcessor)} Cannot create new Cyclops fire! PrefabSpawn component could not be found!]");
                return;
            }

            component.SpawnManual(delegate(GameObject fireGO)
            {
                Fire componentInChildren = fireGO.GetComponentInChildren<Fire>();
                if (componentInChildren)
                {
                    componentInChildren.fireSubRoot = subFire.subRoot;
                    NitroxEntity.SetNewId(componentInChildren.gameObject, fireData.FireId);
                }
            });

            subFire.roomFires = roomFiresDict;
            subFire.availableNodes = availableNodes;
        }

        public void Create(GenericFireData fireData)
        {
            if (NitroxEntity.TryGetObjectFrom(fireData.FireId, out _))
            {
                return;
            }

            using (PacketSuppressor<FireCreated>.Suppress())
            {
                // Using standard Fire TechType for spawning generic fires.
                GameObject fireGo = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/Doodads/BaseFire"), fireData.Position.ToUnity(), fireData.Rotation.ToUnity());
                if (!fireGo)
                {
                    Log.Error("Could not load BaseFire prefab for generic fire sync!");
                    return;
                }

                NitroxEntity.SetNewId(fireGo, fireData.FireId);
                if (fireData.ParentId != null && NitroxEntity.TryGetObjectFrom(fireData.ParentId, out GameObject parent))
                {
                    fireGo.transform.SetParent(parent.transform, true);
                    Fire fireComp = fireGo.GetComponent<Fire>();
                    if (fireComp)
                    {
                        fireComp.fireSubRoot = parent.GetComponent<SubRoot>();
                    }
                }
            }
        }
    }
}
