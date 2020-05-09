using System.Linq;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands.Abstract
{
    public abstract partial class Command
    {
        public class CallArgs
        {
            public Command Command { get; }
            public string[] Args { get; }
            public Optional<Player> Sender { get; }

            public string SenderName => Sender.HasValue ? Sender.Value.Name : "SERVER";

            public CallArgs(Command command, Optional<Player> sender, string[] args)
            {
                Command = command;
                Sender = sender;
                Args = args;
            }

            public bool IsValid(int index)
            {
                return index < Args.Length && index >= 0 && Args.Length != 0;
            }

            public string Get(int index)
            {
                return Get<string>(index);
            }

            public string GetTillEnd(int startIndex = 0)
            {
                // TODO: Proper argument capture/parse instead of this argument join hack
                if (Args?.Length > 0)
                {
                    return string.Join(" ", Args.Skip(startIndex));
                }

                return string.Empty;
            }

            public T Get<T>(int index)
            {
                IParameter<object> param = Command.Parameters[index];
                string arg = IsValid(index) ? Args[index] : null;

                if (arg == null)
                {
                    return default(T);
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)arg;
                }

                return (T)param.Read(arg);
            }
        }
    }
}
