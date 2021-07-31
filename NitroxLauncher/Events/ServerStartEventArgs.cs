using System;

namespace NitroxLauncher.Events
{
    public sealed class ServerStartEventArgs : EventArgs
    {
        public bool IsEmbedded { get; }

        public ServerStartEventArgs(bool embedded)
        {
            IsEmbedded = embedded;
        }

        public override string ToString()
        {
            return $"[ServerStartEventArgs - IsEmbedded: {IsEmbedded}]";
        }
    }
}
