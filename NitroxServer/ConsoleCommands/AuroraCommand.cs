using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

// TODO: When we have the command system, we'll need to move this stuff to the new system
public class AuroraCommand : Command
{
    private readonly EventTriggerer eventTriggerer;

    // We shouldn't let the server use this command because it needs some stuff to happen client-side like goals
    public AuroraCommand(EventTriggerer eventTriggerer) : base("aurora", Perms.ADMIN, PermsFlag.NO_CONSOLE, "Manage Aurora's state")
    {
        AddParameter(new TypeString("countdown/restore/explode", true, "Which action to apply to Aurora"));

        this.eventTriggerer = eventTriggerer;
    }

    protected override void Execute(CallArgs args)
    {
        string action = args.Get<string>(0);

        switch (action.ToLower())
        {
            case "countdown":
                eventTriggerer.ExplodeAurora(true);
                break;
            case "restore":
                eventTriggerer.RestoreAurora();
                break;
            case "explode":
                eventTriggerer.ExplodeAurora(false);
                break;
            default:
                // Same message as in the abstract class, in method TryExecute
                SendMessage(args.Sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                break;
        }
    }
}
