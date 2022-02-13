using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class ServerStoppedProcessor : ClientPacketProcessor<ServerStopped>
{
    public override void Process(ServerStopped packet)
    {
        // This packet is just meant to tell the modal to modify its message
        if (LostConnectionModal.Instance)
        {
            LostConnectionModal.ServerStopped = true;
        }
    }
}
