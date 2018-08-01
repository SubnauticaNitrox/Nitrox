using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Debugger
    {
        private readonly IPacketSender packetSender;

        public Debugger(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void SceneDebuggerChange(string path, int gameObjectID, int componentID, string fieldName, object value)
        {
            SceneDebuggerChange packet = new SceneDebuggerChange(path, gameObjectID, componentID, fieldName, value);
            packetSender.Send(packet);
        }
    }
}
