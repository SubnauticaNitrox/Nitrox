using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.GameLogic.Settings;
using NitroxModel.Networking;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

public class ClockSyncInitialSyncProcessor : InitialSyncProcessor
{
    private readonly TimeManager timeManager;
    private readonly NtpSyncer ntpSyncer;
    private readonly LiteNetLibClient liteNetLibClient;

    public ClockSyncInitialSyncProcessor(TimeManager timeManager, NtpSyncer ntpSyncer, IClient client)
    {
        this.timeManager = timeManager;
        this.ntpSyncer = ntpSyncer;
        liteNetLibClient = (LiteNetLibClient)client;

        AddStep(initialSync => NTPSync(initialSync.TimeData.TimePacket));
    }

    public IEnumerator NTPSync(TimeChange timeData)
    {
        timeManager.SetServerCorrectionData(timeData.OnlineMode, timeData.UtcCorrectionTicks);

        ntpSyncer.Setup(true);
        ntpSyncer.RequestNtpService();

        yield return new WaitUntil(() => ntpSyncer.Finished);

        if (ntpSyncer.OnlineMode)
        {
            timeManager.SetClientCorrectionData(true, ntpSyncer.CorrectionOffset);
            // If server AND client are in online mode, we have everything we need
            if (timeData.OnlineMode)
            {
                // TODO: when the thing is ready, write instead: yield break;
            }
        }
        else
        {
            // TODO: Start a retry (every minute for example) interval IF the server is in online mode, else when the server switches to online mode, start the said interval
        }

        // TODO: Fix modal not appearing
        //yield return Modal.Get<InfoModal>().ShowAsync("Currently OFFLINE. Relying on a fallback clock sync method which might be");

        Log.Warn($"Both client ({(ntpSyncer.OnlineMode ? "ONLINE" : "OFFLINE")}) and server ({(timeData.OnlineMode ? "ONLINE" : "OFFLINE")}) aren't in ONLINE mode. Falling back to {nameof(ClockSyncProcedure)}");
        // TODO: maybe only show the modal when the below procedure failed and instead
        yield return GetAveragePing();
    }

    /// <summary>
    /// 5 seconds procedure to calculate an average time delta with the server
    /// </summary>
    private IEnumerator GetAveragePing()
    {
        Log.Debug("GetAveragePing()");
        int procedureDuration = (int)NitroxPrefs.OfflineClockSyncDuration.Value; // seconds
        using ClockSyncProcedure clockSyncProcedure = ClockSyncProcedure.Start(liteNetLibClient, procedureDuration);
        yield return new WaitForSecondsRealtime(procedureDuration);
        bool success = clockSyncProcedure.TryGetSafeAverageRTD(out long remoteTimeDelta);

        Log.Info($"[success: {success}] calculated RTD: {remoteTimeDelta}");
        timeManager.SetCorrectionDelta(remoteTimeDelta);
    }
}
