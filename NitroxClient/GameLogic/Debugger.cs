using LitJson;
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
            SceneDebuggerChange packet = new(path, gameObjectID, componentID, fieldName, JsonMapper.ToJson(value));
            packetSender.Send(packet);
        }
    }
}
