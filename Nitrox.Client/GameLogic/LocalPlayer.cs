using System;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Nitrox.Client.GameLogic
{
    public class LocalPlayer : ILocalNitroxPlayer
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Lazy<GameObject> body;
        private readonly Lazy<GameObject> playerModel;
        private readonly Lazy<GameObject> bodyPrototype;

        public GameObject Body => body.Value;

        public GameObject PlayerModel => playerModel.Value;

        public GameObject BodyPrototype => bodyPrototype.Value;

        public string PlayerName => multiplayerSession.AuthenticationContext.Username;
        public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
            body = new Lazy<GameObject>(() => Player.main.RequireGameObject("body"));
            playerModel = new Lazy<GameObject>(() => Body.RequireGameObject("player_view"));
            bodyPrototype = new Lazy<GameObject>(CreateBodyPrototype);
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
        {
            PlayerStats playerStats = new PlayerStats(multiplayerSession.Reservation.PlayerId, oxygen, maxOxygen, health, food, water, infectionAmount);
            packetSender.Send(playerStats);
        }

        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleMovementData> vehicle)
        {
            Movement movement;
            if (vehicle.HasValue)
            {
                movement = new VehicleMovement(multiplayerSession.Reservation.PlayerId, vehicle.Value);
            }
            else
            {
                movement = new Movement(multiplayerSession.Reservation.PlayerId, location.ToDto(), velocity.ToDto(), bodyRotation.ToDto(), aimingRotation.ToDto());
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(multiplayerSession.Reservation.PlayerId, (int)type, (int)state);
            packetSender.Send(animEvent);
        }

        public void BroadcastDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(multiplayerSession.AuthenticationContext.Username, deathPosition.ToDto());
            packetSender.Send(playerDeath);
        }

        public void BroadcastSubrootChange(Optional<NitroxId> subrootId)
        {
            SubRootChanged packet = new SubRootChanged(multiplayerSession.Reservation.PlayerId, subrootId);
            packetSender.Send(packet);
        }
        public void BroadcastEscapePodChange(Optional<NitroxId> escapePodId)
        {
            EscapePodChanged packet = new EscapePodChanged(multiplayerSession.Reservation.PlayerId, escapePodId);
            packetSender.Send(packet);
        }

        public void BroadcastWeld(NitroxId id, float healthAdded)
        {
            WeldAction packet = new WeldAction(id, healthAdded);
            packetSender.Send(packet);
        }

        private GameObject CreateBodyPrototype()
        {
            GameObject prototype = Body;

            // Cheap fix for showing head, much easier since male_geo contains many different heads
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
            GameObject clone = Object.Instantiate(prototype);
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            clone.SetActive(false);

            return clone;
        }
    }
}
