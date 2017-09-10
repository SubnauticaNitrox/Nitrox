using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionBeginProcessor : ClientPacketProcessor<DeconstructionBegin>
    {
        public override void Process(DeconstructionBegin packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if(opGameObject.IsPresent())
            {
                GameObject deconstructing = opGameObject.Get();
                Constructable constructable = deconstructing.GetComponent<Constructable>();

                if(constructable != null)
                {
                    constructable.SetState(false, false);
                }
                else
                {
                    Console.WriteLine("Gameobject did not have a valid constructable component!");
                }
            }
            else
            {
                Console.WriteLine("Could not find game object to deconstruct: " + packet.Guid);
            }
        }
    }
}
