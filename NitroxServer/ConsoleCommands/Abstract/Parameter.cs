using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract class Parameter<T> : IParameter<T>
    {
        public bool IsRequired { get; }
        public string Name { get; }
        private string Description { get; }

        protected Parameter(string name, bool isRequired, string description)
        {
            Validate.IsFalse(string.IsNullOrEmpty(name));

            Name = name;
            IsRequired = isRequired;
            Description = description;
        }

        public abstract bool IsValid(string arg);
        public abstract T Read(string arg);

        public virtual string GetDescription()
        {
            return Description;
        }

        public override string ToString()
        {
            return $"{(IsRequired ? '{' : '[')}{Name}{(IsRequired ? '}' : ']')}";
        }
    }

    public interface IParameter<out T>
    {
        bool IsRequired { get; }
        string Name { get; }

        bool IsValid(string arg);
        T Read(string arg);
        string GetDescription();
    }
}
