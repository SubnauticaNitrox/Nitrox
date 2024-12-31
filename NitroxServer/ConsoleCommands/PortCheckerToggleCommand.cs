using LiteNetLib;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Communication.LiteNetLib;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands;
public class PortCheckerToggleCommand : Command
{
    public PortCheckerToggleCommand() : base("toggleportchecker", Perms.CONSOLE, "Enable/Disable the port forwarding tester")
    {
        
    }

    protected override void Execute(CallArgs args)
    {
        PortCheckerSupport.active = !PortCheckerSupport.active;
        Log.Info("Togggled port checker " + PortCheckerSupport.active);
    }
}
