using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

// TODO: When we make the new command system, move this stuff to it
public class SunbeamCommand : Command
{
    private readonly StoryManager storyManager;

    // We shouldn't let the server use this command because it needs some stuff to happen client-side like goals
    public SunbeamCommand(StoryManager storyManager) : base("sunbeam", Perms.ADMIN, PermsFlag.NO_CONSOLE, "Start sunbeam events")
    {
        AddParameter(new TypeString("storystart/countdown/gunaim", true, "Which Sunbeam event to start"));

        this.storyManager = storyManager;
    }

    protected override void Execute(CallArgs args)
    {
        if (!args.Sender.HasValue)
        {
            SendMessage(args.Sender, "This command can't be used by CONSOLE");
            return;
        }
        string action = args.Get<string>(0);

        switch (action.ToLower())
        {
            case "storystart":
                storyManager.StartSunbeamEvent(PlaySunbeamEvent.SunbeamEvent.STORYSTART);
                break;
            case "countdown":
                storyManager.StartSunbeamEvent(PlaySunbeamEvent.SunbeamEvent.COUNTDOWN);
                break;
            case "gunaim":
                storyManager.StartSunbeamEvent(PlaySunbeamEvent.SunbeamEvent.GUNAIM);
                break;
            default:
                // Same message as in the abstract class, in method TryExecute
                SendMessage(args.Sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                break;
        }
    }
}
