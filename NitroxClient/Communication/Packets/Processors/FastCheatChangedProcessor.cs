using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FastCheatChangedProcessor : ClientPacketProcessor<FastCheatChanged>
{
    public override void Process(FastCheatChanged packet)
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
    }
}
