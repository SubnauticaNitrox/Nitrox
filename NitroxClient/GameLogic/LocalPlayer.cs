using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.Rendering;

namespace NitroxClient.GameLogic
{
    public class LocalPlayer : ILocalNitroxPlayer
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;

        public GameObject Body { get; }
        public GameObject PlayerModel { get; }
        public GameObject BodyPrototype { get; }
        public PlayerContext PlayerContext { get; }

        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;

            // get player information
            string username = this.multiplayerSession.AuthenticationContext.Username;
            ushort playerid = this.multiplayerSession.Reservation.PlayerId;
            PlayerSettings settings = this.multiplayerSession.PlayerSettings;

            // encapsulate player info in a PlayerContext so it can be accessed and passed along easily.
            PlayerContext = new PlayerContext(username, playerid, settings);

            Body = Player.main.RequireGameObject("body");
            PlayerModel = Body.RequireGameObject("player_view");

            BodyPrototype = CreateBodyPrototype();
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(PlayerContext, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }

        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleMovementData> vehicle)
        {
            Movement movement;

            if (vehicle.IsPresent())
            {
                movement = new VehicleMovement(PlayerContext, vehicle.Get());
            }
            else
            {
                movement = new Movement(PlayerContext, location, velocity, bodyRotation, aimingRotation);
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(PlayerContext, (int)type, (int)state);
            packetSender.Send(animEvent);
        }

        public void BroadcastDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(PlayerContext, deathPosition);
            packetSender.Send(playerDeath);
        }

        public void BroadcastSubrootChange(Optional<string> subrootGuid)
        {
            SubRootChanged packet = new SubRootChanged(PlayerContext, subrootGuid);
            packetSender.Send(packet);
        }

        private GameObject CreateBodyPrototype()
        {
            GameObject prototype = Body;

            // Cheap fix for showing head, much easier since male_geo contains many different heads
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
            GameObject clone = Object.Instantiate(prototype);
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            return clone;
        }
    }
}
