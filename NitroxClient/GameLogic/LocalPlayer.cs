using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic
{
    public class LocalPlayer : ILocalNitroxPlayer
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;
        private readonly Lazy<GameObject> body;
        private readonly Lazy<GameObject> playerModel;
        private readonly Lazy<GameObject> bodyPrototype;

        public GameObject Body => body.Value;

        public GameObject PlayerModel => playerModel.Value;

        public GameObject BodyPrototype => bodyPrototype.Value;

        public string PlayerName => multiplayerSession.AuthenticationContext.Username;
        public ushort PlayerId => multiplayerSession.Reservation.PlayerId;
        public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

        public Perms Permissions;
        
        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender, ThrottledPacketSender throttledPacketSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;
            body = new Lazy<GameObject>(() => Player.main.RequireGameObject("body"));
            playerModel = new Lazy<GameObject>(() => Body.RequireGameObject("player_view"));
            bodyPrototype = new Lazy<GameObject>(CreateBodyPrototype);
            Permissions = Perms.PLAYER;
        }

        public void BroadcastLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleMovementData> vehicle)
        {
            Movement movement;
            if (vehicle.HasValue)
            {
                movement = new VehicleMovement(multiplayerSession.Reservation.PlayerId, vehicle.Value);
            }
            else
            {
                movement = new PlayerMovement(multiplayerSession.Reservation.PlayerId, location.ToDto(), velocity.ToDto(), bodyRotation.ToDto(), aimingRotation.ToDto());
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state) => packetSender.Send(new AnimationChangeEvent(multiplayerSession.Reservation.PlayerId, (int)type, (int)state));

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount) => packetSender.Send(new PlayerStats(multiplayerSession.Reservation.PlayerId, oxygen, maxOxygen, health, food, water, infectionAmount));

        public void BroadcastDeath(Vector3 deathPosition) => packetSender.Send(new PlayerDeathEvent(multiplayerSession.Reservation.PlayerId, deathPosition.ToDto()));

        public void BroadcastSubrootChange(Optional<NitroxId> subrootId) => packetSender.Send(new SubRootChanged(multiplayerSession.Reservation.PlayerId, subrootId));

        public void BroadcastEscapePodChange(Optional<NitroxId> escapePodId) => packetSender.Send(new EscapePodChanged(multiplayerSession.Reservation.PlayerId, escapePodId));

        public void BroadcastWeld(NitroxId id, float healthAdded) => packetSender.Send(new WeldAction(id, healthAdded));

        public void BroadcastHeldItemChanged(NitroxId itemId, PlayerHeldItemChanged.ChangeType techType, NitroxTechType isFirstTime) => packetSender.Send(new PlayerHeldItemChanged(multiplayerSession.Reservation.PlayerId, itemId, techType, isFirstTime));

        public void BroadcastQuickSlotsBindingChanged(NitroxId[] slotItemIds) => throttledPacketSender.SendThrottled(new PlayerQuickSlotsBindingChanged(slotItemIds), (packet) => 1);

        private GameObject CreateBodyPrototype()
        {
            GameObject prototype = Body;

            // Cheap fix for showing head, much easier since male_geo contains many different heads
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
            GameObject clone = Object.Instantiate(prototype, Multiplayer.Main.transform, false);
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            clone.SetActive(false);
            clone.name = "RemotePlayerPrototype";

            // Removing items that are held in hand
            foreach (Transform child in clone.transform.Find($"player_view/{PlayerEquipmentConstants.ITEM_ATTACH_POINT_GAME_OBJECT_NAME}"))
            {
                if (!child.gameObject.name.Contains("attach1_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            return clone;
        }
    }
}
