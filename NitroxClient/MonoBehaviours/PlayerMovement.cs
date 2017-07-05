using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerMovement : MonoBehaviour
    {
        private float time = 0.0f;
        public float interpolationPeriod = 0.05f;
        
        public void Update()
        {
            time += Time.deltaTime;
            
            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                time = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Quaternion rotation = Player.main.transform.rotation;
                Optional<VehicleModel> vehicle = GetAndTrackVehicle();
               
                Multiplayer.PacketSender.UpdatePlayerLocation(currentPosition, rotation, vehicle);
            }
        }
        
        private Optional<VehicleModel> GetAndTrackVehicle()
        {
            Vehicle vehicle = Player.main.GetVehicle();

            if(vehicle == null)
            {
                return Optional<VehicleModel>.Empty();
            }
            
            ManagedMultiplayerObject managedObject = vehicle.gameObject.GetComponent<ManagedMultiplayerObject>();

            if (managedObject == null)
            {
                managedObject = vehicle.gameObject.AddComponent<ManagedMultiplayerObject>();
            }

            Quaternion rotation = vehicle.gameObject.transform.rotation;
            TechType techType = CraftData.GetTechType(vehicle.gameObject);
            VehicleModel model = new VehicleModel(Enum.GetName(typeof(TechType), techType), managedObject.GUID, ApiHelper.Quaternion(rotation));

            return Optional<VehicleModel>.Of(model);
        }
    }
}
