using System;
using System.Threading;

namespace NitroxClient.GameLogic.PictureFrames;

public sealed class SessionByteBudget(Func<long> capBytesProvider)
{
    private long consumedBytes;
    private int capReachedLogged;
    
    public static long MbToBytes(float megabytes) => (long)(megabytes * 1024f * 1024f);

    public long ConsumedBytes => Interlocked.Read(ref consumedBytes);
    public bool HasBudget => ConsumedBytes < capBytesProvider();

    public void Consume(long byteCount) => Interlocked.Add(ref consumedBytes, byteCount);
    
    public bool TryMarkCapReachedOnce() => !HasBudget && Interlocked.CompareExchange(ref capReachedLogged, 1, 0) == 0;
}
