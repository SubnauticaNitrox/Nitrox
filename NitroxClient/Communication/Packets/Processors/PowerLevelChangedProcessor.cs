using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PowerLevelChangedProcessor : ClientPacketProcessor<PowerLevelChanged>
    {
        public override void Process(PowerLevelChanged packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
                        
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
        
    }
}
