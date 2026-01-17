using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Gui.Modals;
using Nitrox.Model.Networking;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

internal sealed class ClockSyncInitialSyncProcessor : InitialSyncProcessor
{
    private readonly TimeManager timeManager;
    private readonly NtpSyncer ntpSyncer;
    private readonly LiteNetLibClient liteNetLibClient;

    public ClockSyncInitialSyncProcessor(TimeManager timeManager, NtpSyncer ntpSyncer, IClient client)
    {
        this.timeManager = timeManager;
        this.ntpSyncer = ntpSyncer;
        liteNetLibClient = (LiteNetLibClient)client;

        AddStep(initialSync => NtpSync(initialSync.TimeData.TimePacket));
    }

    public IEnumerator NtpSync(TimeChange timeData)
    {
        timeManager.SetServerCorrectionData(timeData.OnlineMode, timeData.UtcCorrectionTicks);

        ntpSyncer.Setup(optionalLogger: Log.CreateLogger<NtpSyncer>());
        ntpSyncer.RequestNtpService();

        yield return new WaitUntil(() => ntpSyncer.IsComplete);

        if (ntpSyncer.OnlineMode)
        {
            timeManager.SetClientCorrectionData(true, ntpSyncer.CorrectionOffset);
            // If server AND client are in online mode, we have everything we need
            if (timeData.OnlineMode)
            {
                yield break;
            }
        }

        Log.Warn($"Both client ({(ntpSyncer.OnlineMode ? "ONLINE" : "OFFLINE")}) and server ({(timeData.OnlineMode ? "ONLINE" : "OFFLINE")}) aren't in ONLINE mode. Falling back to {nameof(ClockSyncProcedure)}");
        
        yield return GetAveragePing();
    }

    /// <summary>
    /// Procedure to calculate an average time delta with the server
    /// </summary>
    private IEnumerator GetAveragePing()
    {
        int procedureDuration = (int)NitroxPrefs.OfflineClockSyncDuration.Value; // seconds
        using ClockSyncProcedure clockSyncProcedure = ClockSyncProcedure.Start(liteNetLibClient, procedureDuration);
        yield return new WaitForSecondsRealtime(procedureDuration);
        bool success = clockSyncProcedure.TryGetSafeAverageRtd(out long remoteTimeDelta);

        Log.Info($"[success: {success}] calculated RTD: {remoteTimeDelta}");
        timeManager.SetCorrectionDelta(remoteTimeDelta);

        if (!success)
        {
            yield return Modal.Get<InfoModal>().ShowAsync("Clock desync fixer failed. Ensure both you and the server are connected to the internet. Or try increasing the \"Offline Clock Sync Duration\" value in the settings, and restart your game.");
        }
    }
}
