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
            addParameter(TypeString.Get, "day/night", false);
        }

        public override void Perform(Optional<Player> sender)
        {
            string time = getArgAt(0);

            switch (time.ToLower())
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
