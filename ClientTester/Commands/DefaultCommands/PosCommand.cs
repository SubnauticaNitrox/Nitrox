using System;

namespace ClientTester.Commands.DefaultCommands
{
    public class PosCommand : NitroxCommand
    {
        public PosCommand()
        {
            Name = "pos";
            Description = "Gets the player's location.";
            Syntax = "pos";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            Console.WriteLine(client.clientPos.ToString());
        }
    }
}
