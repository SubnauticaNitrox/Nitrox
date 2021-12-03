using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.Constants;
using NitroxModel.Logger;

namespace NitroxServer.Communication
{
    public static class LANDiscoveryServer
    {
        private static readonly byte[] responseData = Encoding.UTF8.GetBytes(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING);

        private static Task listenTask;

        public static void Start()
        {
            listenTask = Task.Run(Listen);
        }

        private static void Listen()
        {
            using UdpClient client = new(LANDiscoveryConstants.BROADCAST_PORT);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, LANDiscoveryConstants.BROADCAST_PORT));

            while (true)
            {
                IPEndPoint requestEndPoint = new(0, 0);
                byte[] requestData = client.Receive(ref requestEndPoint);
                string requestString = Encoding.UTF8.GetString(requestData);

                if (requestString == LANDiscoveryConstants.BROADCAST_REQUEST_STRING) // security check
                {
                    Log.Info($"Client at {requestEndPoint} discovered this server through LAN.");
                    client.Connect(requestEndPoint);
                    client.Send(responseData, responseData.Length, requestEndPoint);
                    client.Close();
                }
            }
        }

        public static void Stop()
        {
            listenTask.Dispose();
        }
    }
}
