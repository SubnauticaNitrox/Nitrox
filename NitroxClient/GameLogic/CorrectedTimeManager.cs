using System;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxModel.Networking;

namespace NitroxClient.GameLogic;

public partial class TimeManager
{
    private readonly NtpSyncer ntpSyncer;

    /// <summary>
    /// Whether or not the local player could obtain a correction from the global NTP server
    /// </summary>
    private bool clientOnlineMode;
    /// <summary>
    /// Local's UTC correction with a global NTP server ("real"). Client Correction = Real UTC Time - Client UTC Time
    /// </summary>
    private TimeSpan clientCorrection;


    /// <summary>
    /// Whether or not server could obtain a correction from the global NTP server
    /// </summary>
    private bool serverOnlineMode;
    /// <summary>
    /// Server's UTC correction with a global NTP server ("real"). Server Correction = Real UTC Time - Server UTC Time
    /// </summary>
    private TimeSpan serverCorrection;

    /// <summary>
    /// Correction Delta = Server UTC Time - Client UTC Time. Calculated thanks to <see cref="ClockSyncProcedure"/>
    /// </summary>
    private TimeSpan correctionDelta;

    public DateTimeOffset ServerUtcNow()
    {
        if (clientOnlineMode && serverOnlineMode)
        {
            // From clientCorrection and serverCorrection we deduce the following equation:
            // Server UTC Time + Server correction = Client UTC Time + Client correction
            // from this equation we deduce Server UTC Time which is the below value
            return DateTimeOffset.UtcNow + clientCorrection - serverCorrection;
        }

        // In any other case than the above one, we can only rely on the clock sync procedure for which the equation gives the below value
        return DateTimeOffset.UtcNow + correctionDelta;
    }

    public void SetCorrectionDelta(long remoteTimeDelta)
    {
        correctionDelta = new TimeSpan(remoteTimeDelta);
        Log.Info($"OFFLINE mode: delta = {correctionDelta}");
    }

    public void SetClientCorrectionData(bool clientOnlineMode, TimeSpan correctionOffset)
    {
        this.clientOnlineMode = clientOnlineMode;
        clientCorrection = correctionOffset;
        Log.Info($"Client ONLINE: correction = {correctionOffset}");
    }

    public void SetServerCorrectionData(bool serverOnlineMode, long serverUtcCorrectionTicks)
    {
        this.serverOnlineMode = serverOnlineMode;
        serverCorrection = new(serverUtcCorrectionTicks);
        Log.Info($"Server ONLINE: correction = {serverCorrection}");
    }

    public void AttemptNtpSync()
    {
        ntpSyncer.Setup(false, (onlineMode, correction) =>
        {
            if (onlineMode)
            {
                SetClientCorrectionData(onlineMode, correction);
            }
        });
        ntpSyncer.RequestNtpService();
    }
}
