using System;

namespace NitroxServer.Wpf
{
    public class ProgramOutputReceivedEventArgs : EventArgs
    {
        public string Text { get; }

        public ProgramOutputReceivedEventArgs(string text)
        {
            Text = text;
        }
    }
}
