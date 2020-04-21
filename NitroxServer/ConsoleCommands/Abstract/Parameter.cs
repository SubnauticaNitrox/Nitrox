using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Parameter<T> : IParameter<T>
    {
        public virtual T DefaultValue => default(T);
        public bool IsRequired { get; }
        public string Name { get; }

        protected Parameter(string name, bool isRequired)
        {
            Validate.IsFalse(string.IsNullOrEmpty(name));

            Name = name;
            IsRequired = isRequired;
        }

        public abstract bool IsValid(string arg);

        public abstract T Read(string arg);

        public override string ToString()
        {
            return $"{(IsRequired ? '{' : '[')}{Name}{(IsRequired ? '}' : ']')}";
        }
    }

    public interface IParameter<out T>
    {
        string Name { get; }
        bool IsRequired { get; }
        T DefaultValue { get; }
        T Read(string arg);

        bool IsValid(string arg);
    }
}
