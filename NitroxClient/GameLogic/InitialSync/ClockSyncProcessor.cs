using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.Networking;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

public class ClockSyncProcessor : InitialSyncProcessor
{
    private readonly TimeManager timeManager;
    private readonly NtpSyncer ntpSyncer;
    private readonly LiteNetLibClient liteNetLibClient;

    public ClockSyncProcessor(TimeManager timeManager, NtpSyncer ntpSyncer, IClient client)
    {
        this.timeManager = timeManager;
        this.ntpSyncer = ntpSyncer;
        liteNetLibClient = (LiteNetLibClient)client;

        AddStep(initialSync => NTPSync(initialSync.OnlineMode));
    }

    public IEnumerator NTPSync(bool onlineMode)
    {
        if (onlineMode)
        {
            ntpSyncer.Setup(true);
            ntpSyncer.RequestNtpService();
            yield return new WaitUntil(() => ntpSyncer.Finished);
            if (ntpSyncer.OnlineMode)
            {
                Log.Info($"Got correction offset: {ntpSyncer.CorrectionOffset}");
                timeManager.SetClockCorrection(ntpSyncer.CorrectionOffset);
                //yield break;
            }
        }
        
        // TODO: Fix modal not appearing
        //yield return Modal.Get<InfoModal>().ShowAsync("Currently OFFLINE. Relying on a fallback clock sync method which might be");
        yield return GetAveragePing();
    }

    /// <summary>
    /// 5 seconds procedure to calculate an average time delta with the server
    /// </summary>
    private IEnumerator GetAveragePing()
    {
        Log.Debug("GetAveragePing()");
        using ClockSyncProcedure clockSyncProcedure = ClockSyncProcedure.Start(liteNetLibClient);
        yield return new WaitForSecondsRealtime(5);
        long remoteTimeDelta = clockSyncProcedure.GetSafeAverageRTD();
        Log.Info($"Calculated RTD: {remoteTimeDelta}");
        timeManager.SetClockCorrection(remoteTimeDelta);
    }
}
