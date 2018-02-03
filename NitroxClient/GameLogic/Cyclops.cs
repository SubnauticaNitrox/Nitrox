using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly IPacketSender packetSender;

        public Cyclops(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ToggleInternalLight(string guid, bool isOn)
        {
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(guid, isOn);
            packetSender.Send(packet);
        }

        public void ToggleFloodLights(string guid, bool isOn)
        {
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(guid, isOn);
            packetSender.Send(packet);
        }

        public void BeginSilentRunning(string guid)
        {
            CyclopsBeginSilentRunning packet = new CyclopsBeginSilentRunning(guid);
            packetSender.Send(packet);
        }

        public void ChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(guid, mode);
            packetSender.Send(packet);
        }

        public void ToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(guid, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void ActivateHorn(string guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(guid);
            packetSender.Send(packet);
        }

        public void ActivateShield(string guid)
        {
            CyclopsActivateShield packet = new CyclopsActivateShield(guid);
            packetSender.Send(packet);
        }

        /// <summary>
        /// Spawn a new Cyclops. <paramref name="subRoot"/> will not have its position or rotation declared upon spawning. You must pull those values from
        /// elsewhere.
        /// </summary>
        public void SpawnNew(GameObject subRoot, Vector3 position, Quaternion rotation)
        {
            string guid = GuidHelper.GetGuid(subRoot.gameObject);
            VehicleMovement packet = new VehicleMovement(packetSender.PlayerId, position, Vector3.zero, rotation, Vector3.zero, TechType.Cyclops, guid, 0, 0, false);
            packetSender.Send(packet);
        }

        /// <summary>
        /// Send a notice to the server to update the damage points on a Cyclops.
        /// 
        /// This method should only handle added/removed damage points, but until we can sync the damage points between clients by syncing the random number generators, we'll
        /// have to get by with this. As a bonus, this is also used as a verification call. If an update packet is lost, the next one will fully sync the damage again.
        /// </summary>
        public void UpdateExternalDamagePoints(SubRoot subRoot)
        {
            string guid = GuidHelper.GetGuid(subRoot.gameObject);
            Validate.NotNull(subRoot.damageManager, "Cyclops Guid: " + guid + " has a null 'CyclopsExternalDamageManager'!");

            // We need all of the active damage point guids to fully sync the damage between computers. SubRoot.damageManager.CreatePoint() is going to create
            // its own random points of damage on all clients as the Cyclops is damaged. When the "winning" damage point list is chosen by the server and redistributed,
            // we will need a list of all of the points that should be active, otherwise each client will have a damage points randomly located.
            List<int> damagePointIndexesList = new List<int>();
            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i].gameObject.activeSelf)
                {
                    damagePointIndexesList.Add(i);
                }
            }

            // We include the health because the "host" computer that called for an update on the damage points is now the one who dictates the overall
            // state of the Cyclops. Damage points are the only way to repair a Cyclops, so reporting the health is only meant to satisfy the logic checks
            // the client does to make sure there are the correct number of damage points for the Cyclop's current health. If there's too many or too few, it will 
            // remove/add damage points. This made for an incredibly hard to track and irritating bug. It has to be the LiveMixin located in the DamageManager. The
            // LiveMixin on SubRoot will not work.
            CyclopsExternalDamage packet = new CyclopsExternalDamage(guid, subRoot.damageManager.subLiveMixin.health, damagePointIndexesList.ToArray());
            packetSender.Send(packet);
        }
    }
}
