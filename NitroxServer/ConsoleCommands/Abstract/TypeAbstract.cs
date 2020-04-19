namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class TypeAbstract<T>
    {
        protected string Name => typeof(T).FullName;
        protected System.Type Type => typeof(T);
        protected T DefaultValue { get; set; } = default(T);

        public abstract bool IsValid(string arg);
        public abstract T Read(string arg);
    }
}
