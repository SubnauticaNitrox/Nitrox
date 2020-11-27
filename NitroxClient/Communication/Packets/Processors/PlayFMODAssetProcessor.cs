using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODAssetProcessor : ClientPacketProcessor<PlayFMODAsset>
    {
        public override void Process(PlayFMODAsset packet)
        {
#pragma warning disable 618
            FMODUWE.PlayOneShot(packet.AssetPath, packet.Position.ToUnity(), 1f);
#pragma warning restore 618
        }
    }
}
