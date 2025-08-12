using System;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxServer.Communication.LiteNetLib;

public class LiteNetLibConnection : INitroxConnection, IEquatable<LiteNetLibConnection>
{
    private readonly NetDataWriter dataWriter = new();
    private readonly NetPeer peer;

    public IPEndPoint Endpoint => peer;
    public NitroxConnectionState State => peer.ConnectionState.ToNitrox();

    public LiteNetLibConnection(NetPeer peer)
    {
        this.peer = peer;
    }

    public void SendPacket(Packet packet)
    {
        if (peer.ConnectionState == ConnectionState.Connected)
        {
            byte[] packetData = packet.Serialize();
            dataWriter.Reset();
            dataWriter.Put(packetData.Length);
            dataWriter.Put(packetData);

            peer.Send(dataWriter, (byte)packet.UdpChannel, NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
        }
        else
        {
            Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peer as IPEndPoint}");
        }
    }

    public static bool operator ==(LiteNetLibConnection left, LiteNetLibConnection right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LiteNetLibConnection left, LiteNetLibConnection right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((LiteNetLibConnection)obj);
    }

    public override int GetHashCode()
    {
        return peer?.Id.GetHashCode() ?? 0;
    }

    public bool Equals(LiteNetLibConnection other)
    {
        return peer?.Id == other?.peer?.Id;
    }
}
