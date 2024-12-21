using System.Net;
using LiteNetLib.Layers;
using System.Net.Sockets;
namespace NitroxServer.Communication.LiteNetLib;

public class PortCheckerSupport : PacketLayerBase
{
    public static bool active;
    private readonly UdpClient udpClient = new();
    public PortCheckerSupport() : base(0)
    {
        active = false;
    }
    
    public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
    {
        if (active)
        {
            Log.Warn("WARNING: Port Checker must be disabled to allow players to join the server");
            byte[] datacopy = (byte[])data.Clone();
            data = [];
            udpClient.Send(datacopy, datacopy.Length, endPoint);
        }
        
    }
    public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
    {

    }
}
