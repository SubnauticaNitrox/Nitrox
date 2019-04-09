using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using System;

namespace NitroxServer.ConsoleCommands
{
    public enum SaveMode{
        ONLYAUTO,
        ONLYEXIT,
        NORMAL,
        MANUAL
    }
    internal class SaveCommand : Command
    {
        public static SaveMode saveMode = SaveMode.NORMAL;

        public SaveCommand() : base("save", Perms.ADMIN, "<optional:option (type save help or save -h for options)>", "Saves the server status. (Or optionally configures auto-save.)", new[] {"saveconfig", "configsave", "autosaveoptions"})
        {
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            if((args.Length == 0)||(args[0] == "now")||(args[0] == "-n")){
                Server.Instance.Save();
                Log.InfoIG("Server state saved!");
            }
            else{
                if(args[0] == "onlyauto"){
                    saveMode = SaveMode.ONLYAUTO;
                    Server.Instance.EnablePeriodicSaving();
                    Log.WarnIG("Save mode toggled to onlyauto! Be careful!");
                }
                else if(args[0] == "onlyexit"){
                    saveMode = SaveMode.ONLYEXIT;
                    Server.Instance.DisablePeriodicSaving();
                    Log.WarnIG("Save mode toggled to onlyexit! Be careful!");
                }
                else if(args[0] == "normal"){
                    saveMode = SaveMode.NORMAL;
                    Server.Instance.EnablePeriodicSaving();
                    Log.InfoIG("Save mode toggled to normal.")
                }
                else if(args[0] == "manual"){
                    saveMode = SaveMode.MANUAL;
                    Server.Instance.DisablePeriodicSaving();
                    Log.WarnIG("Save mode toggled to manual! Be (extremly) careful!");
                }
                else if((args[0] == "help")||(args[0] == "-h")){
                    Log.InGame("Look at the console :-) (If you typed this CMD)"); //TODO: Make it send only to person, who typed it.
                    Log.Info("Showing detailed help for save command... (MAY not work in WPF mode)")
                    Console.WriteLine("help / -h - Show this help message");
                    Console.WriteLine("(no params) / -n / now - saves server status");
                    Console.WriteLine("manual - Auto-save: OFF Save-on-exit system: OFF");
                    Console.WriteLine("normal - Auto-save: ON Save-on-exit system: ON");
                    Console.WriteLine("onlyexit - Auto-save: OFF Save-on-exit system: ON");
                    Console.WriteLine("onlyauto - Auto-save: ON Save-on-exit system: OFF");
                    Console.WriteLine("(wrong param) - Shows error message.");
                    Log.Info("Help diplayed.")
                }
                else{
                    Log.ErrorIG("Save mode toggled to "+args[0]+". Or at least there was an attempt. Because this mode does not exist. Lol. Be (more intelligent and) careful or use save -h.")
                }
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return ((args.Length == 0)||(args.Length == 1));
        }
    }
}
