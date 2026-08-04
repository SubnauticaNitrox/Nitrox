using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.ScannerRooms;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ScannerRoomSnapshotPageProcessor(ScannerRoomManager scannerRoomManager) : IClientPacketProcessor<ScannerRoomSnapshotPage>
{
    private readonly ScannerRoomManager scannerRoomManager = scannerRoomManager;

    public Task Process(ClientProcessorContext context, ScannerRoomSnapshotPage packet)
    {
        scannerRoomManager.ProcessPage(packet);
        return Task.CompletedTask;
    }
}
