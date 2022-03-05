using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class ServerStoppedProcessor : ClientPacketProcessor<ServerStopped>
{
    public override void Process(ServerStopped packet)
    {
        Modal.Get<ServerStoppedModal>()?.Show();
    }
}
