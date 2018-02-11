using System;
using NitroxModel.Helper;

namespace NitroxModel.NitroxConsole.Events
{
    public class ConsoleInputEventArgs : EventArgs
    {
        public string Input { get; }

        public ConsoleInputEventArgs(string input)
        {
            Validate.NotNull(input); // User text can't be null so this shouldn't happen.

            Input = input;
        }
    }
}
