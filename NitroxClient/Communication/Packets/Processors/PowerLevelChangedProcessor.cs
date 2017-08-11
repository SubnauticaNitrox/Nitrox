using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PowerLevelChangedProcessor : ClientPacketProcessor<PowerLevelChanged>
    {
        public override void Process(PowerLevelChanged packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if (opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();

                if (packet.PowerType == PowerType.ENERGY_INTERFACE)
                {
                    EnergyInterface energyInterface = gameObject.GetComponent<EnergyInterface>();

                    if(energyInterface != null)
                    {
                        energyInterface.ModifyCharge(packet.Amount);
                    }
                    else
                    {
                        Console.WriteLine("Energy interface was not found on that game object!");
                    }
                }
                else
                {
                    Console.WriteLine("Unsupported packet power type: " + packet.PowerType);
                }
            }
            else
            {
                Console.WriteLine("Could not locate game object with guid: " + packet.Guid);
            }
        }
        
    }
}
