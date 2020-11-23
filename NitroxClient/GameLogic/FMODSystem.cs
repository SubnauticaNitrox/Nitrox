using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class FMODSystem
    {
        private readonly IPacketSender packetSender;

        public FMODSystem(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void PlayFMODAsset(string id, NitroxVector3 position, float radius)
        {
            PlayFMODAsset packet = new PlayFMODAsset(id, position, radius);
            packetSender.Send(packet);
        }
    }
}
