namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class TypeAbstract<T>
    {
        protected string Name => typeof(T).FullName;
        protected System.Type Type => typeof(T);

        public abstract bool isValid(string arg);
        public abstract T read(string arg);
    }
}
