using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.Lidgren;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxModel.Packets;

namespace NitroxClient.Communication.NetworkingLayer
{
    public class ModulatingClient : IClient
    {
        public bool IsConnected { get; private set; }

        private IClient CurrentClient { get; set; }
        private readonly Queue<IClient> fallbackClients;

        public ModulatingClient()
        {
            CurrentClient = new LiteNetLibClient();
            fallbackClients = new Queue<IClient>();
            fallbackClients.Enqueue(new LidgrenClient());
        }

        public void Start(string ipAddress, int serverPort)
        {
            CurrentClient.Start(ipAddress, serverPort);
            IsConnected = CurrentClient.IsConnected;

            if (!IsConnected && fallbackClients.Any())
            {
                SwitchToFallback();
                Start(ipAddress, serverPort);
            }
        }

        public void Stop()
        {
            CurrentClient.Stop();
        }

        public void Send(Packet packet)
        {
            CurrentClient.Send(packet);
        }

        private void SwitchToFallback()
        {
            CurrentClient.Stop();
            CurrentClient = fallbackClients.Dequeue();
        }
    }
}
