using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using Math = System.Math;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerPositionInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;

        public PlayerPositionInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // Make sure the player is configured
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Players can be spawned in buildings
            DependentProcessors.Add(typeof(EscapePodInitialSyncProcessor)); // Players can be spawned in escapePod
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Players can be spawned in vehicles
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            // We freeze the player so that he doesn't fall before the cells around him have loaded
            Player.main.cinematicModeActive = true;

            Vector3 position = packet.PlayerSpawnData.ToUnity();
            Quaternion rotation = packet.PlayerSpawnRotation.ToUnity();
            if (Math.Abs(position.x) < 0.0002 && Math.Abs(position.y) < 0.0002 && Math.Abs(position.z) < 0.0002)
            {
                position = Player.mainObject.transform.position;
            }
            Player.main.SetPosition(position, rotation);

            PositionMaintainer updater = Player.mainObject.AddComponent<PositionMaintainer>();

            // Player.Update is setting SubRootID to null after Player position is set
            using (packetSender.Suppress<EscapePodChanged>())
            {
                Player.main.ValidateEscapePod();
            }

            // Player position is relative to a subroot if in a subroot
            Optional<NitroxId> subRootId = packet.PlayerSubRootId;
            if (!subRootId.HasValue)
            {
                yield return Terrain.WaitForWorldLoad();
                yield break;
            }

            Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Value);
            if (!sub.HasValue)
            {
                Log.Error("Could not spawn player into subroot with id: " + subRootId.Value);
                yield return Terrain.WaitForWorldLoad();
                yield break;
            }

            if (!sub.Value.TryGetComponent(out SubRoot subRoot))
            {
                Log.Debug("SubRootId-GameObject has no SubRoot component, so it's assumed to be the EscapePod");
                yield return Terrain.WaitForWorldLoad();
                yield break;
            }

            // If player is not swimming
            Player.main.SetCurrentSub(subRoot);
            if (subRoot.isBase)
            {
                // If the player's in a base, we don't need to wait for the world to load
                Player.main.cinematicModeActive = false;
                yield break;
            }

            Transform rootTransform = subRoot.transform;
            Quaternion vehicleAngle = rootTransform.rotation;
            // "position" is a relative position and "positionInVehicle" an absolute position
            Vector3 positionInVehicle = vehicleAngle * position + rootTransform.position;
            Player.main.SetPosition(positionInVehicle, rotation * vehicleAngle);

            if (subRoot.isCyclops)
            {
                updater.Initialize(subRoot, position, rotation);
                updater.SetEnabled(true);

                void DisableComponent()
                {
                    updater.SetEnabled(false);
                    updater.Deinitialize();
                    Multiplayer.OnLoadingCompleted -= DisableComponent;
                }

                Multiplayer.OnLoadingCompleted += DisableComponent;
            }
            else
            {
                Player.main.cinematicModeActive = false;
            }
        }
        
        /// <summary>
        /// Maintains the local player at certain relative location inside a subRoot, even if it moves.
        /// </summary>
        public class PositionMaintainer : MonoBehaviour
        {
            private SubRoot subRoot;
            private Vector3 position;
            private Quaternion rotation;

            public void Initialize(SubRoot subRoot, Vector3 position, Quaternion rotation)
            {
                this.subRoot = subRoot;
                this.position = position;
                this.rotation = rotation;
            }

            public void Deinitialize()
            {
                subRoot = null;
                position = default;
                rotation = default;
            }

            public void LateUpdate()
            {
                if (!subRoot)
                {
                    return;
                }
                Player.main.SetPosition(CalculatePosition(), CalculateRotation());
            }

            public void SetEnabled(bool state)
            {
                Player.main.cinematicModeActive = state;
                enabled = state;
            }

            public Vector3 CalculatePosition()
            {
                Transform rootTransform = subRoot.transform;
                Quaternion vehicleAngle = rootTransform.rotation;
                return vehicleAngle * position + rootTransform.position;
            }

            public Quaternion CalculateRotation()
            {
                return subRoot.transform.rotation * rotation;
            }
        }
    }
}
