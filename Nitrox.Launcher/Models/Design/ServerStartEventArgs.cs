using System;

namespace Nitrox.Launcher.Models.Design;

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
