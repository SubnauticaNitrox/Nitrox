using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly EventTriggerer eventTriggerer;

        public TimeCommand(EventTriggerer eventTriggerer) : base("time", Perms.MODERATOR, "Changes the map time")
        {
            AddParameter(new TypeString("day/night", false, "Time to change to"));

            this.eventTriggerer = eventTriggerer;
        }

        protected override void Execute(CallArgs args)
        {
            string time = args.Get(0);

            switch (time?.ToLower())
            {
                case "day":
                    eventTriggerer.ChangeTime(EventTriggerer.TimeModification.DAY);
                    SendMessageToAllPlayers("Time set to day");
                    break;

                case "night":
                    eventTriggerer.ChangeTime(EventTriggerer.TimeModification.NIGHT);
                    SendMessageToAllPlayers("Time set to night");
                    break;

                default:
                    eventTriggerer.ChangeTime(EventTriggerer.TimeModification.SKIP);
                    SendMessageToAllPlayers("Skipped time");
                    break;
            }
        }
    }
}
