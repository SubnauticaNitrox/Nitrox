using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    class HelpCommand : Command
    {
        public HelpCommand() : base("help", Optional<string>.Empty(), "Shows help about all the commands.", new[] {"commands", "cmds", "all"})
        {

        }

        public override void RunCommand(string[] args)
        {
            Log.Info("Showing all commands...\n"); // Extra "\n" for bigger gap.
            Command[] commands = new Command[] { this, new ListCommand(null), new ExitCommand(), new KickCommand(null, null), new SaveCommand(), new SayCommand(null) };
            foreach(Command cmd in commands)
            {
                Console.WriteLine("Command: "+cmd.Name+":\n-----\nInfo: "+cmd.Description+"\nArgs: "+cmd.Args.Get()+"\n"+
                    "Source: Nitrox\n" //Just in case. May be useful if mod engine come to Subnautica some day. Right now, left it as it is. Just "Source: Nitrox"
                    +"Alternative commands:");
                foreach (string alt in cmd.Alias) //ALT = altrnative
                {
                    Console.WriteLine(alt);
                }
                Console.WriteLine(); // Extra WriteLine for bigger gap.
            }
            Log.Info("Command list ended.");
        }

        public override bool VerifyArgs(string[] args)
        {
            if (args.Length == 0)
            {
                return true;
            }

            else
            {
                Log.Error("You can't get info about single command yet, or arguments you typed aren't commands.");
                return false;
            }
        }
    }
}
