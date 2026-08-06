using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.ScannerRooms;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ScannerRoomScanStateChangedProcessor(ScannerRoomManager scannerRoomManager) : IClientPacketProcessor<ScannerRoomScanStateChanged>
{
    private readonly ScannerRoomManager scannerRoomManager = scannerRoomManager;

    public Task Process(ClientProcessorContext context, ScannerRoomScanStateChanged packet)
    {
        scannerRoomManager.ApplyScanState(packet.MapRoomId, packet.CanonicalState);
        return Task.CompletedTask;
    }
}
