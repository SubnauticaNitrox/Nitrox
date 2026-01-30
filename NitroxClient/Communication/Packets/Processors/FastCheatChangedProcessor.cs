using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FastCheatChangedProcessor : IClientPacketProcessor<FastCheatChanged>
{
    public Task Process(ClientProcessorContext context, FastCheatChanged packet)
    {
        switch (packet.Cheat)
        {
            case FastCheatChanged.FastCheat.HATCH:
                NoCostConsoleCommand.main.fastHatchCheat = packet.Value;
                break;

            case FastCheatChanged.FastCheat.GROW:
                NoCostConsoleCommand.main.fastGrowCheat = packet.Value;
                break;
        }
        return Task.CompletedTask;
    }
}
