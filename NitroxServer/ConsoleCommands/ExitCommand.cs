using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    internal class ExitCommand : Command
    {
        public ExitCommand() : base("exit", Perms.ADMIN, "<optional:force>", "Exits the server", new[] {"stop", "halt", "quit", "abort", "shutdown", "poweroff"})
        {
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            if ((args.Length == 0)&&((SaveCommand.saveMode == SaveMode.NORMAL)||(SaveCommand.saveMode == SaveMode.ONLYEXIT))){
                Log.WarnIG("Stopping server!");
                Server.Instance.Stop();
            }
            else if ((args.Length == 0)&&((SaveCommand.saveMode == SaveMode.MANUAL)||(SaveCommand.saveMode == SaveMode.ONLYAUTO))){
                Log.WarnIG("Stopping server!");
                Log.ErrorIG("Error stopping server: It seems, that save-on-exit system is disabled. Your server won't stop in this case. To force it to shutdown, please type exit force.");
            }
            else if (args[0] == "force"){
                Log.WarnIG("Stopping server!");
                Server.Instance.Stop_NOSAVE();
            }
            else{
                Log.WarnIG("Stopping server!");
                Log.ErrorIG("Error stopping server: invalid param!");
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return ((args.Length == 0)||(args.Length == 1));
        }
    }
}
