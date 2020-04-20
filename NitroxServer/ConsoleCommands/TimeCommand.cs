using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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

        protected override void Perform(Optional<Player> sender)
        {
            string time = ReadArgAt(0);

            switch (time?.ToLower())
            {
                case "day":
                    timeKeeper.SetDay();
                    SendMessageToBoth(sender, "Time set to day");
                    break;

                case "night":
                    timeKeeper.SetNight();
                    SendMessageToBoth(sender, "Time set to night");
                    break;

                default:
                    timeKeeper.SkipTime();
                    SendMessageToBoth(sender, "Skipped time");
                    break;
            }
        }
    }
}
