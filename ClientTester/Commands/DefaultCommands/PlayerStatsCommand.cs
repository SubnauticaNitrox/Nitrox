using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace ClientTester.Commands.DefaultCommands
{
    class PlayerStatsCommand : NitroxCommand
    {
        private readonly IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();

        public PlayerStatsCommand()
        {
            Name = "stats";
            Description = "Broadcast stats package";
            Syntax = "stats <player> [oxygen [maxOxygen [health [food [water]]]]]";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            AssertMinimumArgs(args, 1);

            IEnumerable<object> numericArgs = args
                .Skip(1)
                .Select(x =>
                {
                    float r;
                    if (float.TryParse(x, out r))
                    {
                        return (object)r;
                    }

                    throw new InvalidArgumentException($"{x} is not a number!");
                });
            numericArgs = numericArgs.Concat(Enumerable.Repeat((object)1f, 5 - numericArgs.Count()));

            PlayerStats playerStats = (PlayerStats)Activator.CreateInstance(typeof(PlayerStats), new object[] { args[0] }.Concat(numericArgs).ToArray());

            packetSender.Send(playerStats);
        }
    }
}
