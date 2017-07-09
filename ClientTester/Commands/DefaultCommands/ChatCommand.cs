using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class ChatCommand : NitroxCommand
    {
        public ChatCommand()
        {
            Name = "chat";
            Description = "Sends a chat message to other players.";
            Syntax = "chat <message>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            if (args.Length < 1)
            {
                CommandManager.NotEnoughArgumentsMessage(1, Syntax);
                return;
            }

            if (args.Length >= 2)
            {
                client.PacketSender.SendChatMessage(String.Join(" ", args.ToArray())); //does not support double spaces!
            }
            else
            {
                client.PacketSender.SendChatMessage(args[0]);
            }
        }
    }
}
