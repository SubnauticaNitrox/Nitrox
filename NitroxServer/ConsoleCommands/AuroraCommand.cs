using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

// TODO: When we make the new command system, move this stuff to it
public class AuroraCommand : Command
{
    private readonly StoryManager storyManager;

    // We shouldn't let the server use this command because it needs some stuff to happen client-side like goals
    public AuroraCommand(StoryManager storyManager) : base("aurora", Perms.ADMIN, PermsFlag.NO_CONSOLE, "Manage Aurora's state")
    {
        AddParameter(new TypeString("countdown/restore/explode", true, "Which action to apply to Aurora"));

        this.storyManager = storyManager;
    }

    protected override void Execute(CallArgs args)
    {
        string action = args.Get<string>(0);

        switch (action.ToLower())
        {
            case "countdown":
                storyManager.ExplodeAurora(true);
                break;
            case "restore":
                storyManager.RestoreAurora();
                break;
            case "explode":
                storyManager.ExplodeAurora(false);
                break;
            default:
                // Same message as in the abstract class, in method TryExecute
                SendMessage(args.Sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                break;
        }
    }
}
