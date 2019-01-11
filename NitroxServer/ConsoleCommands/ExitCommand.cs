using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using System;
using System.Threading;

namespace NitroxServer.ConsoleCommands
{
    public class ExitCommand : Command
    {
        private bool ConfirmedUnstable = false;
        private bool ConfirmedNosave = false;
        private bool ConfirmedOld = false;

        public ExitCommand() : base("exit", Optional<string>.Of("<method>"), "Exits the server", new[] {"stop", "halt", "quit", "abort"})
        {
        }

        public override void RunCommand(string[] args)
        {
            if (args[0] == "s")
            {
                Log.Info("Launched \"Exit\" with \"s\" param, which is shortcut from \"save\". Saving world...");
                Server.Instance.Save();
                Log.Info("World saved. Stopping server...");
                Server.Instance.StopNosave();
                Log.Info("Server stopped.");
            }
            else if (args[0] == "n")
            {
                Log.Info("Launched \"Exit\" with \"n\" param, which is shortcut from \"not save\". ALL UNSAVED PROGRESS WILL BE LOST!!!");
                if (ConfirmedNosave)
                {
                    ConfirmedNosave = false;
                    Log.Info("Stopping server...");
                    Server.Instance.StopNosave();
                    Log.Info("Server stopped.");
                }
                else
                {
                    Log.Info("Type \"exit n\" again to confirm this.");
                    ConfirmedNosave = true;
                }
                
            }
            else if (args[0] == "u")
            {
                Log.Warn("Launched \"Exit\" with \"u\" param, which is shortcut from \"unstable\". Executing unstable method is NOT a good idea. It is:\n-Unfinished (Kickall is missing)\n-Depracted (So new features will not be added there & it's not going to be finished)\n-Unstable (It can lead to very bad side effects)\n-lagging server. (Server's program breaks multiple times here, so it will lag in that moments. That's why there was \"kickall\", but unfortunatley \"kickall\" wasn't finished. And it's never going to be finished.)");
                if (ConfirmedUnstable)
                {
                    ConfirmedUnstable = false;
                    RunCommandOLD(args);
                }
                else
                {
                    Log.Info("Type \"exit u\" again to confirm this.");
                    ConfirmedUnstable = true;
                }
                
            }
            else if (args[0] == "o")
            {
                Log.Warn("Launched \"Exit\" with \"o\" param, which is shortcut from \"old\". Executing old save method isn't a good idea. It is depracted (So new features will not be added there).");
                if (ConfirmedOld)
                {
                    ConfirmedOld = false;
                    RunCommandVERYOLD(args);
                }
                else
                {
                    Log.Info("Type \"exit o\" again to confirm this.");
                    ConfirmedOld = true;
                }
                
            }
            else if (args[0] == "h")
            {
                Console.WriteLine("Showing all params:");
                Console.WriteLine("o - shortcut from \"old\" - starts the old, well-known stop method. WARNING: IT'S DEPRACTED!");
                Console.WriteLine("n - shortcut from \"Not save\" - Exits without saving.");
                Console.WriteLine("s - shortcut from \"Save\" - Saves & exit.");
                Console.WriteLine("h - shortcut from \"Help\" - Shows help.");
                Console.WriteLine("u - shortcut from \"unstable\" - starts the UNSTABELE stop method. WARNING: Executing unstablee method is NOT a good idea. It is:\n-Unfinished (Kickall is missing)\n-Depracted (So new features will not be added there & it's not going to be finished)\n-Unstable (It can lead to very bad side effects)\n-lagging server. (Server's program breaks multiple times here, so it will lag in that moments. That's why there was \"kickall\", but unfortunatley \"kickall\" wasn't finished. And it's never going to be finished.)\n-ACTUALLY THE WORST METHOD YOU CAN EXECUTE!");
            }
            else
            {
                Log.Error("Wrong param. Type \"exit h\" for params list.");
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            if(args.Length == 0)
            {
                Log.Error("Old exit command is no longer avaible. Please execute \"exit\" command with params. Type \"exit h\" for params list.");
                return false;
            }
            else if (args.Length == 1)
            {
                return true;
            }
            else
            {
                Log.Error("Wrong param. Type \"exit h\" for params list.");
                return false;
            }
        }




        //Old save methods
        public void RunCommandVERYOLD(string[] args)
        {
            Server.Instance.Stop();
        }

        public void RunCommandOLD(string[] args)
        {
            Console.WriteLine("Stopping server:");
            Console.WriteLine("Kicking all players...");
            //TODO: Kicking all
            Console.WriteLine("Players kicking failed. They'll kicked after server stop anyway.");
            Console.WriteLine("Calling \"Save\" method...");
            Console.WriteLine("Method called. Your anwser was: " + Save());
            Console.WriteLine("Exiting...");
            Server.Instance.StopNosave();
            Console.WriteLine("Exited.");
            Console.WriteLine("Server stopped!");
        }










        //My experiment (I decided to keep it and mark as deparcted, unstabe and unfinished. Because it is depracted, unstable and unfinished)
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
