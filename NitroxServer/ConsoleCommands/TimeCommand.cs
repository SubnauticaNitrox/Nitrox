using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
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
