using System.Collections.Generic;
using System.Timers;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class FireManager
    {
        private const float THRESHOLD = 3f; // Limits the rate at which packets are sent
 
        Dictionary<Fire, float> queue = new Dictionary<Fire, float>();
        
        private readonly IPacketSender packetSender;

        public FireManager(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void UpdateFire(Fire fire, float douseDelta)
        {
            float douseAmount = 0;
            queue.TryGetValue(fire, out douseAmount);
            float newDouse = douseAmount + douseDelta;
            bool extinguished = (bool)fire.ReflectionGet("isExtinguished");

            if(newDouse >= THRESHOLD || extinguished)
            {
                packetSender.Send(new FireDouse(fire.transform.position, newDouse, extinguished));
                queue.Remove(fire);
            }
            else
            {
                queue[fire] = newDouse;
            }
        }
    }
}
