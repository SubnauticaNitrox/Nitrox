using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODAssetProcessor : ClientPacketProcessor<PlayFMODAsset>
    {
        private readonly IPacketSender packetSender;

        public PlayFMODAssetProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PlayFMODAsset packet)
        {
            using (packetSender.Suppress<PlayFMODAsset>())
            {
#pragma warning disable 618
                FMODUWE.PlayOneShot(packet.AssetPath, packet.Position.ToUnity(), packet.Volume);
#pragma warning restore 618
            }
        }
    }
}
