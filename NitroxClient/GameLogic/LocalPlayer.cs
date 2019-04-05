using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerModel.Abstract;
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
        public string PlayerName => multiplayerSession.AuthenticationContext.Username;
        public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;

            Body = Player.main.RequireGameObject("body");
            PlayerModel = Body.RequireGameObject("player_view");

            BodyPrototype = CreateBodyPrototype();
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(multiplayerSession.Reservation.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }


        private Vector3 lastMovementLocation;
        private Vector3 lastMovementVelocity;
        private Quaternion lastMovementBodyRotation;
        private Quaternion lastMovementAimingRotation;
        
        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleMovementData> vehicle)
        {
            Movement movement;

            if (vehicle.IsPresent())
            {
                movement = new VehicleMovement(multiplayerSession.Reservation.PlayerId, vehicle.Get());
                packetSender.Send(movement);
            }
            else
            {

                //reduce unneeded net traffic if no movement has happened
                bool _needSend = false;
                if (lastMovementLocation != null && lastMovementBodyRotation != null && lastMovementVelocity != null && lastMovementAimingRotation != null)
                {
                    //use .ToString compare to skip minor changes
                    if(!(lastMovementLocation.ToString().Equals(location.ToString()) && lastMovementBodyRotation.ToString().Equals(bodyRotation.ToString()) && lastMovementVelocity.ToString().Equals(velocity.ToString()) && lastMovementAimingRotation.ToString().Equals(aimingRotation.ToString())))
                    {
                        _needSend = true;
                    }
                }
                else
                {
                    _needSend = true;
                }

                if(_needSend)
                {
#if TRACE && MOVEMENT
                    NitroxModel.Logger.Log.Debug("Send UpdateLocation: playerID: " + multiplayerSession.Reservation.PlayerId + " location: " + location + " velocity: " + velocity + " bodyRotation: " + bodyRotation + " aiming: " + aimingRotation);
#endif

                    lastMovementLocation = location;
                    lastMovementVelocity = velocity;
                    lastMovementBodyRotation = bodyRotation;
                    lastMovementAimingRotation = aimingRotation;

                    movement = new Movement(multiplayerSession.Reservation.PlayerId, location, velocity, bodyRotation, aimingRotation);
                    packetSender.Send(movement);
                }
            }
            
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(multiplayerSession.Reservation.PlayerId, (int)type, (int)state);
            packetSender.Send(animEvent);
        }

        public void BroadcastDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(multiplayerSession.AuthenticationContext.Username, deathPosition);
            packetSender.Send(playerDeath);
        }

        public void BroadcastSubrootChange(Optional<string> subrootGuid)
        {
            SubRootChanged packet = new SubRootChanged(multiplayerSession.Reservation.PlayerId, subrootGuid);
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
