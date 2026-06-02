namespace Nitrox.Server.Subnautica.Models.Helper;

internal record DisposeWrapper(IDisposable? One, IDisposable? Two) : IDisposable
{
    public void Dispose()
    {
        One?.Dispose();
        Two?.Dispose();
    }
}
