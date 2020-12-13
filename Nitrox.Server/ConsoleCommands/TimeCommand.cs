using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly TimeKeeper timeKeeper;

        public TimeCommand(TimeKeeper timeKeeper) : base("time", Perms.ADMIN, "Changes the map time")
        {
            this.timeKeeper = timeKeeper;
            AddParameter(new TypeString("day/night", false));
        }

        protected override void Execute(CallArgs args)
        {
            string time = args.Get(0);

            switch (time?.ToLower())
            {
                case "day":
                    timeKeeper.SetDay();
                    SendMessageToAllPlayers("Time set to day");
                    break;

                case "night":
                    timeKeeper.SetNight();
                    SendMessageToAllPlayers("Time set to night");
                    break;

                default:
                    timeKeeper.SkipTime();
                    SendMessageToAllPlayers("Skipped time");
                    break;
            }
        }
    }
}
