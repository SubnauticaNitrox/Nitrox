using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Signs
    {
        private readonly IPacketSender packetSender;

        public Signs(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SignChanged(Sign sign)
        {
            string guid = GuidHelper.GetGuid(sign.gameObject);
        }
    }
}
