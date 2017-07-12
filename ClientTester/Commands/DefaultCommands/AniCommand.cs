using NitroxClient.MonoBehaviours;
using System;
using System.Linq;
using UnityEngine;

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
            if (args.Length < 2)
            {
                CommandManager.NotEnoughArgumentsMessage(2, Syntax);
                return;
            }
            AnimChangeState state;
            if (args[1] == "on" || args[1] == "1" || args[1] == "true")
            {
                state = AnimChangeState.On;
            } else if (args[1] == "off" || args[1] == "0" || args[1] == "false")
            {
                state = AnimChangeState.Off;
            } else
            {
                state = AnimChangeState.Unset;
            }
            client.PacketSender.AnimationChange((AnimChangeType)int.Parse(args[0]), state);
        }
    }
}
