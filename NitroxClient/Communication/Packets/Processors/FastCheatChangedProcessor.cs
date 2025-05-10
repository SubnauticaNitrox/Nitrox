using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FastCheatChangedProcessor : IClientPacketProcessor<FastCheatChanged>
{
    public Task Process(IPacketProcessContext context, FastCheatChanged packet)
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
