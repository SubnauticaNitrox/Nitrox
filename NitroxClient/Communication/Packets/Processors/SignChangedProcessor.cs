using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SignChangedProcessor : ClientPacketProcessor<SignChanged>
    {
        public override void Process(SignChanged packet)
        {
            Log.InGame("Packet recieved!");
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
            uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>();

            sign.text = packet.NewText;
            sign.colorIndex = packet.ColorIndex;
            sign.elementsState = packet.Elements;
            sign.scaleIndex = packet.ScaleIndex;
            sign.SetBackground(packet.Background);
        }
    }
}
