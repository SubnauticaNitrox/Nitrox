using System;
using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using NitroxClient.Debuggers;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.Modals;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib;

public class LiteNetLibClient : IClient
{
    private const string DisableIpv6EnvKey = "NITROX_DISABLE_IPV6";
    private const string ConnectTimeoutMsEnvKey = "NITROX_CLIENT_CONNECT_TIMEOUT_MS";
    private const string ManualModeEnvKey = "NITROX_LITENETLIB_MANUAL_MODE";
    private const string DisableCrcEnvKey = "NITROX_DISABLE_LITENETLIB_CRC";
    private const string CrcFallbackEnvKey = "NITROX_LITENETLIB_CRC_FALLBACK";
    private const string DisableNativeSocketsEnvKey = "NITROX_DISABLE_LITENETLIB_NATIVE_SOCKETS";

    private readonly EventBasedNetListener listener;
    private readonly bool useManualMode;
    private bool useCrc;
    private NetManager client;

    private readonly AutoResetEvent connectedEvent = new(false);
    private readonly NetDataWriter dataWriter = new();
    private readonly INetworkDebugger networkDebugger;
    private readonly PacketReceiver packetReceiver;
    private readonly FieldInfo manualModeFieldInfo = typeof(NetManager).GetField("_manualMode", BindingFlags.Instance | BindingFlags.NonPublic);

    public bool IsConnected { get; private set; }
    public int PingInterval
    {
        get => client.PingInterval;
        set => client.PingInterval = value;
    }
    public Action<long> LatencyUpdateCallback;

    public LiteNetLibClient(PacketReceiver packetReceiver, INetworkDebugger networkDebugger = null)
    {
        this.packetReceiver = packetReceiver;
        this.networkDebugger = networkDebugger;
        useManualMode = IsTruthyEnvironmentVariable(ManualModeEnvKey);

        listener = new EventBasedNetListener();
        listener.PeerConnectedEvent += Connected;
        listener.PeerDisconnectedEvent += Disconnected;
        listener.NetworkReceiveEvent += ReceivedNetworkData;
        listener.NetworkLatencyUpdateEvent += (peer, _) =>
        {
            LatencyUpdateCallback?.Invoke(peer.RemoteTimeDelta);
        };

        useCrc = NitroxEnvironment.IsReleaseMode && !IsTruthyEnvironmentVariable(DisableCrcEnvKey);
        client = CreateClient();
    }

    public async Task StartAsync(string ipAddress, int serverPort)
    {
        Log.Info("Initializing LiteNetLibClient...");

        // ConfigureAwait(false) is needed because Unity uses a custom "UnitySynchronizationContext". Which makes async/await work like Unity coroutines.
        // Because this Task.Run is async-over-sync this would otherwise blocks the main thread as it wants to, without ConfigureAwait(false), continue on the same thread (i.e. main thread).
        await ConnectAsync(ipAddress, serverPort).ConfigureAwait(false);

        if (!IsConnected && useCrc && IsTruthyEnvironmentVariable(CrcFallbackEnvKey))
        {
            Log.Info("LiteNetLib CRC connection failed; retrying without CRC packet layer");
            client.Stop();
            connectedEvent.Reset();
            useCrc = false;
            client = CreateClient();
            await ConnectAsync(ipAddress, serverPort).ConfigureAwait(false);
        }
    }

    private async Task ConnectAsync(string ipAddress, int serverPort)
    {
        Console.WriteLine($"[Nitrox] LiteNetLib connect start {ipAddress}:{serverPort}; manual={useManualMode}; ipv6={client.IPv6Enabled}; timeout={GetConnectTimeout()}ms; release={NitroxEnvironment.IsReleaseMode}; crc={useCrc}");

        await Task.Run(() =>
        {
            if (useManualMode)
            {
                client.StartInManualMode(0);
            }
            else
            {
                client.Start();
            }

            client.Connect(ipAddress, serverPort, "nitrox");
        }).ConfigureAwait(false);

        WaitForConnection();
        Console.WriteLine($"[Nitrox] LiteNetLib connect finished; connected={IsConnected}; peers={client.ConnectedPeersCount}");
        connectedEvent.Reset();
    }

    public void Send(Packet packet)
    {
        byte[] packetData = packet.Serialize();
        dataWriter.Reset();
        dataWriter.Put(packetData.Length);
        dataWriter.Put(packetData);

        networkDebugger?.PacketSent(packet, dataWriter.Length);
        client.SendToAll(dataWriter, (byte)packet.UdpChannel, NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
    }

    public void Stop()
    {
        IsConnected = false;
        client.Stop();
    }

    /// <summary>
    ///     This should be called <b>once</b> each game tick
    /// </summary>
    public void PollEvents()
    {
        if (useManualMode)
        {
            client.ManualUpdate(0);
        }
        else
        {
            client.PollEvents();
        }
    }

    private void ReceivedNetworkData(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            packetReceiver.Add(packet);
            networkDebugger?.PacketReceived(packet, packetDataLength);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private void Connected(NetPeer peer)
    {
        // IsConnected must happen before Set() so that its state is noticed WHEN we unblock the thread (cf. connectedEvent.WaitOne(...))
        IsConnected = true;
        connectedEvent.Set();
        Log.Info("Connected to server");
        Console.WriteLine("[Nitrox] LiteNetLib connected to server");
    }

    private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Check must happen before IsConnected is set to false, so that it doesn't send an exception when we aren't even ingame
        if (Multiplayer.Active)
        {
            Modal.Get<LostConnectionModal>()?.Show();
        }

        IsConnected = false;
        Log.Info($"Disconnected from server: {disconnectInfo.Reason} ({disconnectInfo.SocketErrorCode})");
        Console.WriteLine($"[Nitrox] LiteNetLib disconnected: {disconnectInfo.Reason} ({disconnectInfo.SocketErrorCode})");
    }

    internal void ForceUpdate()
    {
        int pingInterval = PingInterval;
        // Set PingInterval to 0 so another ping is sent immediately
        PingInterval = 0;
        // ManualUpdate requires the client to have _manualMode set to true so we temporarily do so
        manualModeFieldInfo.SetValue(client, true);
        client.ManualUpdate(0);
        manualModeFieldInfo.SetValue(client, false);
        // We set it back to its high value so another ping isn't sent while we're waiting for the previous one
        PingInterval = pingInterval;
    }

    private void WaitForConnection()
    {
        int timeout = GetConnectTimeout();
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            PollEvents();
            if (connectedEvent.WaitOne(15))
            {
                return;
            }
        }
    }

    private static int GetConnectTimeout()
    {
        string timeoutValue = Environment.GetEnvironmentVariable(ConnectTimeoutMsEnvKey);
        if (int.TryParse(timeoutValue, out int timeout) && timeout > 0)
        {
            return timeout;
        }

        return 2000;
    }

    private NetManager CreateClient()
    {
        return new NetManager(listener, GetPacketLayer())
        {
            UpdateTime = 15,
            ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length,
            IPv6Enabled = !IsTruthyEnvironmentVariable(DisableIpv6EnvKey),
            UseNativeSockets = !IsTruthyEnvironmentVariable(DisableNativeSocketsEnvKey),
#if DEBUG
            DisconnectTimeout = 300_000, //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#else
            DisconnectTimeout = 30_000, // 30 seconds; prevents false disconnects when post-sync game-loading stalls LiteNetLib briefly
#endif
        };
    }

    private PacketLayerBase GetPacketLayer()
    {
        if (!useCrc)
        {
            return null;
        }

        return new Crc32cLayer();
    }

    private static bool IsTruthyEnvironmentVariable(string key)
    {
        string value = Environment.GetEnvironmentVariable(key);
        return value is "1" or "true" or "TRUE" or "True" or "yes" or "YES" or "Yes";
    }
}
