using System;
using System.Buffers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.Abstract;
using NitroxClient.Debuggers;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.Modals;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib;

public class LiteNetLibClient : IClient
{
    private readonly NetManager client;

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
        EventBasedNetListener listener = new();
        listener.PeerConnectedEvent += Connected;
        listener.PeerDisconnectedEvent += Disconnected;
        listener.NetworkReceiveEvent += ReceivedNetworkData;
        listener.NetworkLatencyUpdateEvent += (peer, _) =>
        {
            LatencyUpdateCallback?.Invoke(peer.RemoteTimeDelta);
        };


        client = new NetManager(listener)
        {
            UpdateTime = 15,
            ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length,
#if DEBUG
            DisconnectTimeout = 300000 //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif
        };
    }

    public async Task StartAsync(string ipAddress, int serverPort)
    {
        Log.Info("Initializing LiteNetLibClient...");

        // ConfigureAwait(false) is needed because Unity uses a custom "UnitySynchronizationContext". Which makes async/await work like Unity coroutines.
        // Because this Task.Run is async-over-sync this would otherwise blocks the main thread as it wants to, without ConfigureAwait(false), continue on the same thread (i.e. main thread).
        await Task.Run(() =>
        {
            client.Start();
            client.Connect(ipAddress, serverPort, "nitrox");
        }).ConfigureAwait(false);

        connectedEvent.WaitOne(2000);
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
    public void PollEvents() => client.PollEvents();

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
    }

    private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Check must happen before IsConnected is set to false, so that it doesn't send an exception when we aren't even ingame
        if (Multiplayer.Active)
        {
            Modal.Get<LostConnectionModal>()?.Show();
        }

        IsConnected = false;
        Log.Info("Disconnected from server");
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
}
