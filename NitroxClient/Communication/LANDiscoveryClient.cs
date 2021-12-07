using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Constants;
using NitroxModel.Logger;

namespace NitroxClient.Communication
{
    public static class LANDiscoveryClient
    {
        private static readonly byte[] requestData = Encoding.UTF8.GetBytes(LANDiscoveryConstants.BROADCAST_REQUEST_STRING);
        private static readonly List<IPEndPoint> discoveredServers = new();

        private static Action<IPEndPoint> foundServerCallback;

        public static void SearchForServers(Action<IPEndPoint> callback)
        {
            foundServerCallback = callback;
            discoveredServers.Clear();

            Log.Info("Searching for LAN servers...");

            Task.Run(BroadcastData);
            Task.Run(ReceiveData);
        }

        private static void BroadcastData()
        {
            using UdpClient broadcastClient = new();
            broadcastClient.EnableBroadcast = true;
#if DEBUG
            broadcastClient.ExclusiveAddressUse = false; // for multiple instances
#endif
            broadcastClient.Client.Bind(new IPEndPoint(IPAddress.Broadcast, LANDiscoveryConstants.BROADCAST_PORT));

            // Send four broadcast packets, spaced one second apart
            for (int i = 0; i < 4; i++)
            {
                broadcastClient.Send(requestData, requestData.Length);
                Thread.Sleep(1000);
            }

            Thread.Sleep(6000);
        }

        private static void ReceiveData()
        {
            using UdpClient receiveClient = new(LANDiscoveryConstants.BROADCAST_PORT);
            receiveClient.Client.Bind(new IPEndPoint(IPAddress.Any, LANDiscoveryConstants.BROADCAST_PORT));

            while (true)
            {
                IPEndPoint responseEndPoint = new(0, 0);
                byte[] responseData = receiveClient.Receive(ref responseEndPoint);
                string responseString = Encoding.UTF8.GetString(responseData);

                if (responseString.StartsWith(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING)) // security check
                {
                    IPAddress serverIP = responseEndPoint.Address;
                    int serverPort = int.Parse(responseString.Substring(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING.Length));
                    IPEndPoint serverEndPoint = new(serverIP, serverPort);

                    if (!discoveredServers.Contains(serverEndPoint)) // prevents duplicate entries
                    {
                        Log.Info($"Found LAN server at {serverEndPoint}.");
                        discoveredServers.Add(serverEndPoint);
                        foundServerCallback(serverEndPoint);
                    }
                }
            }
        }
    }
}
