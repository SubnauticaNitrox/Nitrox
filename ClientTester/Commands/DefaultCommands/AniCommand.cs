using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace ClientTester.Commands.DefaultCommands
{
    public class AniCommand : NitroxCommand
    {
        private readonly LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

        public AniCommand()
        {
            Name = "ani";
            Description = "Changes animations.";
            Syntax = "ani <animation number> <state>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            AssertMinimumArgs(args, 2);

            AnimChangeState state;

            if (args[1] == "on" || args[1] == "1" || args[1] == "true")
            {
                state = AnimChangeState.ON;
            }
            else if (args[1] == "off" || args[1] == "0" || args[1] == "false")
            {
                state = AnimChangeState.OFF;
            }
            else
            {
                state = AnimChangeState.UNSET;
            }

            localPlayer.AnimationChange((AnimChangeType)int.Parse(args[0]), state);
        }
    }
}
