using System;
using System.Collections.Generic;
using System.Threading;
using NitroxServerUdpPunch.Communication.NetworkingLayer;
using NitroxServerUdpPunch.Communication.NetworkingLayer.LiteNetLib;

namespace NitroxServerUdpPunch
{
    class Program
    {
        static void Main(string[] args)
        {
            // If may another punch server will be used
            List<IPunchServer> punchServers = new List<IPunchServer> { new LiteNetLibPunchServer(11001, 3) };
            foreach (IPunchServer server in punchServers)
            {
                server.Start();
            }
            while (true)
            {
                foreach (IPunchServer server in punchServers)
                {
                    server.Process();
                }                
                Thread.Sleep(100);
            }
        }
    }
}
