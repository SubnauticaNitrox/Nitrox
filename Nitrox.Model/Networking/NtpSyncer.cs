using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nitrox.Model.Networking;

public sealed class NtpSyncer
{
    /// <summary>
    ///     Allowed time in milliseconds per ntp sync sequence
    /// </summary>
    /// <remarks>
    ///     6 services * 5s TIMEOUT = max 30 seconds waiting if all services are semi-working but fail.
    ///     Also if the client is not connected to the internet, all requests will fail immediately so there'll be no extra
    ///     wait time
    /// </remarks>
    private const float TIMEOUT_INTERVAL = 5000;

    private ILogger<NtpSyncer> logger = NullLogger<NtpSyncer>.Instance;
    private LiteNetLibNtp ntp;

    private Timer? timer;

    public bool IsComplete { get; private set; }
    public bool OnlineMode { get; private set; }
    public TimeSpan CorrectionOffset { get; private set; }
    private Action<bool, TimeSpan>? DisposeCallback { get; set; }

    public void Setup(Action<bool, TimeSpan>? finishCallback = null, ILogger<NtpSyncer>? optionalLogger = null)
    {
        if (optionalLogger != null)
        {
            logger = optionalLogger;
        }
        DisposeCallback = finishCallback;

        ntp = new(TreatPacket);

        timer = new(TIMEOUT_INTERVAL) { AutoReset = false };
        timer.Elapsed += delegate
        {
            ntp.ClearNtpRequests();
            RequestNtpService();
        };
    }

    public void RequestNtpService()
    {
        if (timer == null)
        {
            return;
        }

        try
        {
            if (ntp.CreateNextNtpRequest() == null)
            {
                Complete();
                return;
            }
            timer.Start();
            ntp.TriggerUpdate();
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred during NTP sync sequence with '{ntp.LatestUsedService}', retrying with another one... ({ex.GetType()}: {ex.Message})");
            timer.Stop();
            RequestNtpService();
        }
    }

    public void Complete()
    {
        if (IsComplete)
        {
            return;
        }
        IsComplete = true;

        timer?.Close();
        timer = null;
        ntp.Dispose();
        try
        {
            DisposeCallback?.Invoke(OnlineMode, CorrectionOffset);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        DisposeCallback = null;
    }

    private void TreatPacket(NtpPacket? ntpPacket)
    {
        timer?.Stop();

        if (ntpPacket != null)
        {
            OnlineMode = true;
            CorrectionOffset = ntpPacket.CorrectionOffset;
            Complete();
            logger.LogDebug($"NTP time correction offset: {ntpPacket.CorrectionOffset}");
        }
        else
        {
            logger.LogError($"NTP request error at {ntp.LatestUsedService}, retrying with another service...");
            RequestNtpService();
        }
    }

    private class LiteNetLibNtp : IDisposable
    {
        private static readonly string[] ntpServices =
        [
            "time.windows.com",
            "3.pool.ntp.org",
            "2.pool.ntp.org",
            "1.pool.ntp.org",
            "0.pool.ntp.org",
            "pool.ntp.org"
        ];

        private readonly EventBasedNetListener listener;

        private readonly NetManager netManager;
        private readonly Stack<string> ntpServicesToTest;
        private string? latestUsedService;

        public string LatestUsedService
        {
            get => latestUsedService ?? "no-service";
            private set => latestUsedService = value;
        }

        public LiteNetLibNtp(EventBasedNetListener.OnNtpResponseEvent ntpEvent)
        {
            ntpServicesToTest = new(ntpServices);

            listener = new();
            listener.NtpResponseEvent += ntpEvent;

            netManager = new(listener);
            netManager.Start();
        }

        public void Dispose()
        {
            netManager.Stop();
            listener.ClearNtpResponseEvent();
        }

        /// <remarks>
        ///     It can happen that the requests were corrupted in which cases we want to clear them
        /// </remarks>
        public void ClearNtpRequests()
        {
            LiteNetLibReflection.GetNtpRequests(netManager).Clear();
        }

        public string? CreateNextNtpRequest()
        {
            if (ntpServicesToTest.Count < 1)
            {
                return null;
            }
            string ntpService = ntpServicesToTest.Pop();
            LatestUsedService = ntpService;
            netManager.CreateNtpRequest(ntpService);
            return ntpService;
        }

        public void TriggerUpdate()
        {
            netManager.TriggerUpdate();
        }
    }

    private static class LiteNetLibReflection
    {
        private static readonly FieldInfo ntpRequestsField = typeof(NetManager).GetField("_ntpRequests", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception($"Unable to find _ntpRequests field in {typeof(NetManager).FullName}");

        public static IDictionary GetNtpRequests(NetManager manager) => (IDictionary)ntpRequestsField.GetValue(manager) ?? throw new Exception("Unable to get NtpRequests from LiteNetLib");
    }
}
