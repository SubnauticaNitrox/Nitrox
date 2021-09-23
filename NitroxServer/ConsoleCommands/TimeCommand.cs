using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly ScheduleKeeper scheduleKeeper;

        public TimeCommand(ScheduleKeeper scheduleKeeper) : base("time", Perms.MODERATOR, "Changes the map time")
        {
            AddParameter(new TypeString("day/night", false));

            this.scheduleKeeper = scheduleKeeper;
        }

        protected override void Execute(CallArgs args)
        {
            string time = args.Get(0);

            switch (time?.ToLower())
            {
                case "day":
                    scheduleKeeper.ChangeTime(ScheduleKeeper.TimeModification.DAY);
                    SendMessageToAllPlayers("Time set to day");
                    break;

                case "night":
                    scheduleKeeper.ChangeTime(ScheduleKeeper.TimeModification.NIGHT);
                    SendMessageToAllPlayers("Time set to night");
                    break;

                default:
                    scheduleKeeper.ChangeTime(ScheduleKeeper.TimeModification.SKIP);
                    SendMessageToAllPlayers("Skipped time");
                    break;
            }
        }
    }
}
