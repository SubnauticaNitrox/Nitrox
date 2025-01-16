using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NitroxModel.Networking;

public class NtpSyncer
{
    /// <summary>
    /// Allowed time in milliseconds per ntp sync sequence
    /// </summary>
    /// <remarks>
    /// 6 services * 5s TIMEOUT = max 30 seconds waiting if all services are semi-working but fail.
    /// Also if the client is not connected to the internet, all requests will fail immediately so there'll be no extra wait time
    /// </remarks>
    public const float TIMEOUT_INTERVAL = 5000;
    private static readonly FieldInfo NTP_REQUESTS_FIELD = typeof(NetManager).GetField("_ntpRequests", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly List<string> NTP_SERVICES = [
        "time.windows.com", "3.pool.ntp.org", "2.pool.ntp.org", "1.pool.ntp.org", "0.pool.ntp.org", "pool.ntp.org"
    ];

    private Stack<string> ntpServicesToTest;
    private NetManager netManager;
    private Timer timer;
    private string latestUsedService;
    private bool verbose;

    public bool Finished { get; private set; }
    public bool OnlineMode { get; private set; }
    public TimeSpan CorrectionOffset { get; private set; }
    public Action<bool, TimeSpan> FinishCallback { get; private set; }

    public void Setup(bool verbose, Action<bool, TimeSpan> finishCallback = null)
    {
        ntpServicesToTest = new(NTP_SERVICES);
        this.verbose = verbose;
        FinishCallback = finishCallback;

        EventBasedNetListener listener = new();
        listener.NtpResponseEvent += TreatPacket;

        netManager = new(listener);
        netManager.Start();

        timer = new(TIMEOUT_INTERVAL)
        {
            AutoReset = false
        };

        timer.Elapsed += delegate
        {
            // It can happen that the requests were corrupted in which cases we want to clear them
            IDictionary ntpRequests = (IDictionary)NTP_REQUESTS_FIELD.GetValue(netManager);
            ntpRequests.Clear();

            RequestNtpService();
        };
    }

    public void RequestNtpService()
    {
        if (ntpServicesToTest.Count == 0)
        {
            Dispose();
            return;
        }

        string ntpService = ntpServicesToTest.Pop();

        try
        {
            latestUsedService = ntpService;
            netManager.CreateNtpRequest(ntpService);
            timer.Start();
            netManager.TriggerUpdate();
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Log.Error($"[{nameof(NtpSyncer)}] An error occurred during NTP sync sequence with {ntpService}, retrying with another one... ({ex.GetType()}: {ex.Message})");
            }
            timer.Stop();
            RequestNtpService();
        }
    }

    private void TreatPacket(NtpPacket ntpPacket)
    {
        timer.Stop();

        if (ntpPacket != null)
        {
            OnlineMode = true;
            CorrectionOffset = ntpPacket.CorrectionOffset;
            Dispose();
            if (verbose)
            {
                Log.Info($"[{nameof(NtpSyncer)}] NTP time correction offset: {ntpPacket.CorrectionOffset}");
            }
        }
        else
        {
            if (verbose)
            {
                Log.Error($"[{nameof(NtpSyncer)}] NTP request error at {latestUsedService}, retrying with another service...");
            }
            RequestNtpService();
        }
    }

    public void Dispose()
    {
        timer?.Close();
        timer = null;
        netManager?.Stop();
        netManager = null;
        Finished = true;
        FinishCallback?.Invoke(OnlineMode, CorrectionOffset);
        FinishCallback = null;
    }
}
