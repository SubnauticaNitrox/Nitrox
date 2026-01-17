using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal sealed class TimeCommand : Command
{
    private readonly TimeService timeService;

    public TimeCommand(TimeService timeService) : base("time", Perms.MODERATOR, "Changes the map time")
    {
        AddParameter(new TypeString("day/night", false, "Time to change to"));

        this.timeService = timeService;
    }

    protected override void Execute(CallArgs args)
    {
        string time = args.Get(0);

        switch (time?.ToLower())
        {
            case "day":
                timeService.ChangeTime(StoryManager.TimeModification.DAY);
                SendMessageToAllPlayers("Time set to day");
                break;

            case "night":
                timeService.ChangeTime(StoryManager.TimeModification.NIGHT);
                SendMessageToAllPlayers("Time set to night");
                break;

            default:
                timeService.ChangeTime(StoryManager.TimeModification.SKIP);
                SendMessageToAllPlayers("Skipped time");
                break;
        }
    }
}
