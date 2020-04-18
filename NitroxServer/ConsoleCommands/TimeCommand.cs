using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly TimeKeeper timeKeeper;

        public TimeCommand(TimeKeeper timeKeeper) : base("time", Perms.ADMIN, "Changes the map time")
        {
            this.timeKeeper = timeKeeper;
            addParameter(string.Empty, TypeString.Get, "day/night", true);
        }

        public override void Perform(string[] args, Optional<Player> sender)
        {
            switch (args[0].ToLower())
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
