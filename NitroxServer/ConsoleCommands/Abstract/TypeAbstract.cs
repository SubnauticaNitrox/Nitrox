namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class TypeAbstract<T>
    {
        protected string Name => nameof(T);
        protected T Type { get; set; }

        public abstract bool isValid(string arg);
        public abstract T read(string arg);
    }
}
