using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib;

public sealed class ClockSyncProcedure(LiteNetLibClient liteNetLibClient) : IDisposable
{
    private readonly LiteNetLibClient liteNetLibClient = liteNetLibClient;
    private readonly int previousPingInterval = liteNetLibClient.PingInterval;
    private readonly List<long> deltas = [];

    public static ClockSyncProcedure Start(LiteNetLibClient liteNetLibClient, int procedureDuration)
    {
        ClockSyncProcedure clockSyncProcedure = new(liteNetLibClient);
        liteNetLibClient.PingInterval = (int)TimeSpan.FromSeconds(procedureDuration).TotalMilliseconds;
        liteNetLibClient.LatencyUpdateCallback += clockSyncProcedure.LatencyUpdate;
        liteNetLibClient.ForceUpdate();
        return clockSyncProcedure;
    }

    public void LatencyUpdate(long remoteTimeDelta)
    {
        deltas.Add(remoteTimeDelta);
        liteNetLibClient.ForceUpdate();
    }

    /// <summary>
    /// Get an average of remote delta times gathered until now without the ones which are not in the standard deviation range.
    /// </summary>
    public bool TryGetSafeAverageRTD(out long average)
    {
        if (deltas.Count == 0)
        {
            Log.Error($"[{nameof(ClockSyncProcedure)}] No delta received !");
            average = 0;
            return false; // abnormal situation
        }
        
        double avg = deltas.Average();
        average = (long)avg; // Need to assign from another variable since you can't give a ref to the below lambda
        long standardDeviation = (long)Math.Sqrt(deltas.Average(v => Math.Pow(v - avg, 2)));

        List<long> validValues = [];
        foreach (long delta in deltas)
        {
            if (Math.Abs(delta - average) <= standardDeviation)
            {
                validValues.Add(delta);
            }
        }

        if (validValues.Count == 0)
        {
            Log.Warn($"[{nameof(ClockSyncProcedure)}] No valid value out of {deltas.Count} deltas. Using regular average without filtering.");
            return true; // value is not really meaningful ...
        }

        average = (long)validValues.Average();
        return true;
    }

    public void Dispose()
    {
        liteNetLibClient.LatencyUpdateCallback -= LatencyUpdate;
        liteNetLibClient.PingInterval = previousPingInterval;
    }
}
