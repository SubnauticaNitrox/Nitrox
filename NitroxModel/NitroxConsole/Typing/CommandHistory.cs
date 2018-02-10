using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.NitroxConsole.Events;

namespace NitroxModel.NitroxConsole.Typing
{
    public class CommandHistory
    {
        private const int MAX_SIZE = 16;
        private int cursor;
        private readonly List<CommandEventArgs> data = new List<CommandEventArgs>();

        public void Add(CommandEventArgs e)
        {
            Validate.NotNull(e);

            // Adding duplicates of last history is not needed..
            if (data.Any() && e.Value == data[data.Count - 1].Value)
            {
                return;
            }

            data.Add(e);
            CleanDataToMaxSize();

            // Reset history to now because new history was just written.
            AfterNow();
        }

        public CommandEventArgs Previous()
        {
            DecCursor();
            return data[cursor];
        }

        public CommandEventArgs Next()
        {
            IncCursor();
            return data[cursor];
        }

        public Optional<CommandEventArgs> Now()
        {
            cursor = 0;
            if (data.Any())
            {
                return Optional<CommandEventArgs>.Of(data[cursor]);
            }
            return Optional<CommandEventArgs>.Empty();
        }

        public void AfterNow()
        {
            cursor = data.Count;
        }

        private void IncCursor()
        {
            cursor++;
            if (cursor >= data.Count)
            {
                cursor = data.Count - 1;
            }
        }

        private void DecCursor()
        {
            cursor--;
            if (cursor < 0)
            {
                cursor = 0;
            }
        }

        private void CleanDataToMaxSize()
        {
            while (data.Count > MAX_SIZE)
            {
                data.RemoveAt(0);
            }
        }
    }
}
