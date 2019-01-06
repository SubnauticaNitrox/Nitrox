using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Signs
    {
        private readonly IPacketSender packetSender;

        public Signs(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Changed(uGUI_SignInput sign)
        {
            string guid = sign.gameObject.FindAncestor<PrefabIdentifier>().Id;

            SignMetadata signMetadata = new SignMetadata(guid, sign.text, sign.colorIndex, sign.scaleIndex, sign.elementsState, sign.IsBackground());
            SignChanged signChanged = new SignChanged(signMetadata);
            packetSender.Send(signChanged);
        }
    }
}
