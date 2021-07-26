using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly TimeKeeper timeKeeper;

        public TimeCommand(TimeKeeper timeKeeper) : base("time", Perms.ADMIN, "{day/night}", "Changes the map time")
        {
            this.timeKeeper = timeKeeper;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            switch (args[0].ToLower())
            {
                case "day":
                    timeKeeper.SetDay();
                    Notify(sender, "Time set to day");
                    break;

                case "night":
                    timeKeeper.SetNight();
                    Notify(sender, "Time set to night");
                    break;

                default:
                    Notify(sender, "Cannot set time, invalid parameters (day/night)");
                    break;
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
