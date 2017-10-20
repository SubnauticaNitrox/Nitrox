using NitroxClient.MonoBehaviours.Gui.Settings;
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
            assertMinimumArgs(args, 1);

            if (args.Length >= 2)
            {
                client.Logic.Chat.SendChatMessage(string.Join(" ", args), SettingsManager.GetColor()); //does not support double spaces!
            }
            else
            {
                client.Logic.Chat.SendChatMessage(args[0], SettingsManager.GetColor());
            }
        }
    }
}
