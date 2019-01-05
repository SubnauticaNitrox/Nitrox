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
            Log.Info("Showing all commands...\n"); // I know, that there is arleady "\n" in Log.Info(), but I added bonus one to make gap between Log.Info and commands bigger.
            Command[] commands = new Command[] { new ExitCommand(), new ListCommand(null), new HelpCommand(), new KickCommand(null, null), new SaveCommand(), new SayCommand(null) };
            foreach(Command cmd in commands)
            {
                Console.WriteLine("Command: "+cmd.Name+":\n-----\nInfo: "+cmd.Description+"\nArgs: "+cmd.Args.Get()+"\n"+
                    "Source: Nitrox\n" //Just in case. May be useful if mod engine come to Subnautica some day. Right now, left it as it is. Just "Source: Nitrox"
                    +"Alternative commands:");
                foreach (string alt in cmd.Alias) //ALT is shortcut for "altrnative"
                {
                    Console.WriteLine(alt);
                }
                Console.WriteLine(); // I know, that there is arleady Console.WriteLine, but I added bonus one to make gap between commands bigger.
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
