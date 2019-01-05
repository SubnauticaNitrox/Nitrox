using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using System;
using System.Threading;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        public ExitCommand() : base("exit", Optional<string>.Empty(), "Exits the server", new[] {"stop", "halt", "quit", "abort"})
        {
        }

        public override void RunCommand(string[] args)
        {
            Console.WriteLine("Stopping server:");
            Console.WriteLine("Kicking all players...");
            //TODO: Kicking all
            Console.WriteLine("Players kicking failed. They'll kicked after server stop anyway.");
            Console.WriteLine("Calling \"Save\" method...");
            Console.WriteLine("Method called. Your anwser was: " + Save());
            Console.WriteLine("Exiting...");
            Server.Instance.Stop();
            Console.WriteLine("Exited.");
            Console.WriteLine("Server stopped!");
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
        
        bool Save()
        {
            Console.WriteLine("Save before exiting? [Y/N]");
            char Output = Console.ReadKey(true).KeyChar;
            if (Output == 'y')
            {
                Console.WriteLine("You pressed \"Y\" - Saving...");
                Server.Instance.Save();
                Console.WriteLine("Saved.");
                Console.WriteLine("Exiting in 100 miliseconds.");
                Thread.Sleep(100);
                return true;
            }
            else if (Output == 'n')
            {
                return AreYouSure();
            }
            else
            {
                Console.WriteLine("Worng key. Calling \"Save\" method again...");
                return Save();
            }
        }

        bool AreYouSure()
        {
            Console.WriteLine("Are you sure? ALL UNSAVED PROGRESS WILL BE LOST! [Y/N]");
            char Output = Console.ReadKey(true).KeyChar;
            if (Output == 'y')
            {
                Console.WriteLine("You pressed \"Y\". ALL UNSAVED PROGRESS WILL BE LOST!");
                return false;
            }
            else if (Output == 'n')
            {
                Console.WriteLine("You presed \"N\". Calling \"Save\" method again...");
                return Save();
            }
            else
            {
                Console.WriteLine("Worng key. Calling \"Are you sure\" method again...");
                return AreYouSure();
            }
        }
    }
}
