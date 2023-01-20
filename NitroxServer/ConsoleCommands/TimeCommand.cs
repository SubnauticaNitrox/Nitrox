using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class TimeCommand : Command
    {
        private readonly StoryManager storyManager;

        public TimeCommand(StoryManager storyManager) : base("time", Perms.MODERATOR, "Changes the map time")
        {
            AddParameter(new TypeString("day/night", false, "Time to change to"));

            this.storyManager = storyManager;
        }

        protected override void Execute(CallArgs args)
        {
            string time = args.Get(0);

            switch (time?.ToLower())
            {
                case "day":
                    storyManager.ChangeTime(StoryManager.TimeModification.DAY);
                    SendMessageToAllPlayers("Time set to day");
                    break;

                case "night":
                    storyManager.ChangeTime(StoryManager.TimeModification.NIGHT);
                    SendMessageToAllPlayers("Time set to night");
                    break;

                default:
                    storyManager.ChangeTime(StoryManager.TimeModification.SKIP);
                    SendMessageToAllPlayers("Skipped time");
                    break;
            }
        }
    }
}
