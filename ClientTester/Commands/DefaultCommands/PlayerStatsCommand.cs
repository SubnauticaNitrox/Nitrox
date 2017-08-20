using NitroxModel.Packets;
using System;
using System.Linq;

namespace ClientTester.Commands.DefaultCommands
{
    class PlayerStatsCommand : NitroxCommand
    {
        public PlayerStatsCommand()
        {
            Name = "stats";
            Description = "Broadcast stats package";
            Syntax = "stats <player> [oxygen [maxOxygen [health [food [water]]]]]";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            if (args.Length < 1)
            {
                throw new NotEnoughArgumentsException(1);
            }

            var numericArgs = args
                .Skip(1)
                .Select(x =>
                {
                    float r;
                    if (float.TryParse(x, out r))
                        return (object)r;
                    throw new InvalidArgumentException($"{x} is not a number!");
                });
            numericArgs = numericArgs.Concat(Enumerable.Repeat((object)1f, 5 - numericArgs.Count()));

            PlayerStats playerStats = (PlayerStats)Activator.CreateInstance(typeof(PlayerStats), new object[] { args[0] }.Concat(numericArgs).ToArray());

            client.PacketSender.Send(playerStats);
        }
    }
}
