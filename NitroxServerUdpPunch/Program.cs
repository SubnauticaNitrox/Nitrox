using System;
using System.Threading;

namespace NitroxServerUdpPunch
{
    class Program
    {
        static void Main(string[] args)
        {
            Communication.NetworkingLayer.LiteNetLib.LiteNetLibPunchServer server = new Communication.NetworkingLayer.LiteNetLib.LiteNetLibPunchServer(11001, "NitroxPunch");
            server.Start();
            while (true)
            {
                server.Process();
                Thread.Sleep(100);
            }
        }
    }
}
