using NitroxClient.MonoBehaviours;

namespace ClientTester.Commands.DefaultCommands
{
    public class AniCommand : NitroxCommand
    {
        public AniCommand()
        {
            Name = "ani";
            Description = "Changes animations.";
            Syntax = "ani <animation number> <state>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            assertMinimumArgs(args, 2);

            AnimChangeState state;

            if (args[1] == "on" || args[1] == "1" || args[1] == "true")
            {
                state = AnimChangeState.On;
            }
            else if (args[1] == "off" || args[1] == "0" || args[1] == "false")
            {
                state = AnimChangeState.Off;
            }
            else
            {
                state = AnimChangeState.Unset;
            }

            client.Logic.Player.AnimationChange((AnimChangeType)int.Parse(args[0]), state);
        }
    }
}
