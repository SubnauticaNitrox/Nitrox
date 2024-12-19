using LiteNetLib;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Communication.LiteNetLib;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands;
public class PortCheckerToggleCommand : Command
{
    PortCheckerSupport packetLayer;
    public PortCheckerToggleCommand() : base("toggleportchecker", Perms.ADMIN, PermsFlag.NO_CONSOLE, "Enable the port forwarding tester")
    {
        
    }

    protected override void Execute(CallArgs args)
    {
        Log.Info("Command executed");
        if (packetLayer.active)
        {
            packetLayer.Deactivate();
            return;
        }
        packetLayer.Activate();
        return;
    }
}
