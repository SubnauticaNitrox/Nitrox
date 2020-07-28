using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
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

        public void Changed(uGUI_SignInput sign)
        {
            GameObject gameObject = sign.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);

            SignMetadata signMetadata = new SignMetadata(id, sign.text, sign.colorIndex, sign.scaleIndex, sign.elementsState, sign.IsBackground());
            SignChanged signChanged = new SignChanged(signMetadata);
            packetSender.Send(signChanged);
        }
    }
}
