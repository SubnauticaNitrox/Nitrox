using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// TODO: When we make the new command system, move this stuff to it
internal sealed class AuroraCommand : Command
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
                storyManager.BroadcastExplodeAurora(true);
                break;
            case "restore":
                storyManager.BroadcastRestoreAurora();
                break;
            case "explode":
                storyManager.BroadcastExplodeAurora(false);
                break;
            default:
                // Same message as in the abstract class, in method TryExecute
                SendMessage(args.Sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                break;
        }
    }
}
