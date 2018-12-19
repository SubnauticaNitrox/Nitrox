using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    class SaveCommand : Command
    {
        public SaveCommand()
        {
            Name = "save";
            Args = new string[] { "save" };
        }

        public override void RunCommand(string[] args)
        {
            Server.Instance.Save();
        }

        public override bool VerifyArgs(string[] args)
        {
            if (args.Length == 1)
            {
                return true;
            }
            return false;
        }
    }
}
