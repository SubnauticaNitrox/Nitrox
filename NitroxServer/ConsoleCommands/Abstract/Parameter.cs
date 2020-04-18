using NitroxModel.Helper;

namespace NitroxServer.ConsoleCommands.Abstract
{

    public interface IParameter
    {
        object Value { get; }
        string Name { get; }
        bool IsRequired { get; }
        bool IsOptional { get; }

        string ToString();
    }

    public class Parameter<T> : IParameter
    {
        public T DEFAULT_VALUE { get; set; }

        public TypeAbstract<T> Type { get; set; }

        public object Value => Type;

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public bool IsOptional => !IsRequired;

        public Parameter(T defaultValue, TypeAbstract<T> type, string name, bool isRequired)
        {
            Validate.NotNull(type);
            Validate.IsFalse(string.IsNullOrEmpty(name));

            DEFAULT_VALUE = defaultValue;
            Type = type;
            Name = name;
            IsRequired = isRequired;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}",
                IsRequired ? '{' : '[',
                Name,
                IsRequired ? '}' : ']'
            );
        }
    }
}
