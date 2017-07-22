using System;

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
                client.Logic.Chat.SendChatMessage(String.Join(" ", args)); //does not support double spaces!
            }
            else
            {
                client.Logic.Chat.SendChatMessage(args[0]);
            }
        }
    }
}
