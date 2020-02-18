using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class DayCommand : Command
    {
        public DayCommand() : base("day", Perms.ADMIN, "", "Set map to Daytime")
        {
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            TimeKeeper timeKeeper = NitroxServiceLocator.LocateService<TimeKeeper>();
            timeKeeper.SetDay();
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
