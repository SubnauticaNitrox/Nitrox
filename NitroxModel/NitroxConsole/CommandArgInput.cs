namespace NitroxModel.NitroxConsole
{
    public class CommandArgInput
    {
        public string Name { get; }
        public object Value { get; }
        public Type ValueType { get; }

        public CommandArgInput(string name, object value)
        {
            Name = name;
            Value = value;

            if (value is string)
            {
                ValueType = Type.STRING;
            }
            else
            {
                ValueType = Type.NUMBER;
            }
        }

        public enum Type
        {
            NUMBER,
            STRING
        }
    }
}
