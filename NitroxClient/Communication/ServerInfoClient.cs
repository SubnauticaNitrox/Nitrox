using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Nitrox.Model.Constants;
using Nitrox.Model.DataStructures.GameLogic;

namespace NitroxClient.Communication;

/// <summary>
/// Queries a server we're not yet connected to for pre-join information
/// </summary>
public static class ServerInfoClient
{
    public static async Task<AchievementsMode?> QueryAchievementsModeAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token : cancellationToken;

        AchievementsMode? result = null;
        void ReceivedResponse(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType != UnconnectedMessageType.BasicMessage || !remoteEndPoint.Equals(endpoint) || reader.GetString() != ServerInfoConstants.INFO_RESPONSE_STRING)
            {
                return;
            }
            result = (AchievementsMode)reader.GetByte();
        }

        EventBasedNetListener listener = new();
        NetManager client = new(listener) { AutoRecycle = true, UnconnectedMessagesEnabled = true };
        if (!client.Start())
        {
            return null;
        }
        listener.NetworkReceiveUnconnectedEvent += ReceivedResponse;

        try
        {
            NetDataWriter writer = new();
            writer.Put(ServerInfoConstants.INFO_REQUEST_STRING);
            client.SendUnconnectedMessage(writer, endpoint);

            while (result == null && !cancellationToken.IsCancellationRequested)
            {
                client.PollEvents();
                try
                {
                    await Task.Delay(50, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Loop condition below will exit on next check.
                }
            }
            return result;
        }
        finally
        {
            listener.NetworkReceiveUnconnectedEvent -= ReceivedResponse;
            client.Stop();
        }
    }
}
