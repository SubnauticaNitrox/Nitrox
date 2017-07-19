using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NitroxClient.GameLogic.Helper;

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
                Optional<VehicleModel> vehicle = GetVehicleModel();
                String subGuid = null;

                SubRoot currentSub = Player.main.GetCurrentSub();

                if (currentSub != null)
                {
                    subGuid = GuidHelper.GetGuid(currentSub.gameObject);
                }

                Multiplayer.PacketSender.UpdatePlayerLocation(currentPosition, rotation, vehicle, Optional<String>.OfNullable(subGuid));
            }
        }
        
        private Optional<VehicleModel> GetVehicleModel()
        {
            Vehicle vehicle = Player.main.GetVehicle();

            if(vehicle == null)
            {
                return Optional<VehicleModel>.Empty();
            }

            String guid = GuidHelper.GetGuid(vehicle.gameObject);
            Quaternion rotation = vehicle.gameObject.transform.rotation;
            TechType techType = CraftData.GetTechType(vehicle.gameObject);

            VehicleModel model = new VehicleModel(Enum.GetName(typeof(TechType), techType), guid, ApiHelper.Quaternion(rotation));

            return Optional<VehicleModel>.Of(model);
        }
    }
}
