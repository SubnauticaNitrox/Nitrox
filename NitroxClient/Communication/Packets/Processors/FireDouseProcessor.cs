using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FireDouseProcessor : ClientPacketProcessor<FireDouse>
    {
        private readonly IPacketSender packetSender;

        public FireDouseProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FireDouse packet)
        {
            Optional<Fire> fire = getNearestFire(packet.FirePosition);
            if (fire.IsPresent())
            {
                fire.Get().Douse(packet.DouseAmount);
                if (packet.Extinguished)
                {
                    fire.Get().ReflectionCall("Extinguished");
                }
            }
        }

        private Optional<Fire> getNearestFire(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 1f);
            Optional<Fire> closestFire = Optional<Fire>.Empty();
            float closestFireDistance = -1;
            foreach(Collider col in colliders)
            {
                Fire fire = col.gameObject.GetComponent<Fire>();
                if (fire)
                {
                    float dist = (position - fire.transform.position).sqrMagnitude;
                    if(closestFireDistance == -1 || dist < closestFireDistance)
                    {
                        closestFireDistance = dist;
                        closestFire = Optional<Fire>.Of(fire);
                    }
                }
            }
            return closestFire;
        }
    }
}
