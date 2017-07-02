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
        public float interpolationPeriod = 0.25f;
        
        public void Update()
        {
            time += Time.deltaTime;
            
            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                interpolationPeriod = 0;

                Vector3 currentPosition = Player.main.transform.position;
                Multiplayer.PacketSender.UpdatePlayerLocation(currentPosition);
            }
        }
    }
}
