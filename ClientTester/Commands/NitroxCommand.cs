namespace ClientTester.Commands
{
    public abstract class NitroxCommand
    {
        public string Name;
        public string Description;
        public string Syntax;
        public abstract void Execute(MultiplayerClient client, string[] args);

        protected void AssertMinimumArgs(string[] args, int totalArgs)
        {
            if (args.Length < totalArgs)
            {
                throw new NotEnoughArgumentsException(totalArgs);
            }
        }
    }
}
