using NitroxClient.Communication.Packets.Processors.Abstract;
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
            GameObject deconstructing = GuidHelper.RequireObjectFrom(packet.Guid);
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
    }
}
