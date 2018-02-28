using NitroxClient.Communication;
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

        public void SceneDebuggerChange(string guid, int componentID, int componentID2, string sceneName, object value)
        {
            SceneDebuggerChange packet = new SceneDebuggerChange(guid, componentID, componentID2, sceneName, value);
            packetSender.Send(packet);
        }
    }
}
