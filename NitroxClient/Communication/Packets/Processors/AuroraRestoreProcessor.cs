using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AuroraRestoreProcessor : ClientPacketProcessor<AuroraRestore>
{
    private readonly PDAManagerEntry pdaManagerEntry;

    public AuroraRestoreProcessor(PDAManagerEntry pdaManagerEntry)
    {
        this.pdaManagerEntry = pdaManagerEntry;
    }

    public override void Process(AuroraRestore packet)
    {
        pdaManagerEntry.AuroraExplosionTriggered = false;
        // Same code as in OnConsoleCommand_restoreship, but we prefixed it so we can't call it like this, also we don't need to SetExplodeTime as it's server stuff
        CrashedShipExploder main = CrashedShipExploder.main;
        main.SwapModels(false);
        main.fxControl.StopAndDestroy(0, 0f);
        main.fxControl.StopAndDestroy(1, 0f);
        Log.Debug("Restored aurora");
    }
}
