using System;
using System.Net.NetworkInformation;
using System.Linq;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class Disconnected : IMultiplayerSessionConnectionState
    {
        public MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.DISCONNECTED;

        public async Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            ValidateState(sessionConnectionContext);

            IClient client = sessionConnectionContext.Client;
            string ipAddress = sessionConnectionContext.IpAddress;
            int port = sessionConnectionContext.ServerPort;
            await StartClientAsync(ipAddress, client, port);
            EstablishSessionPolicy(sessionConnectionContext, client);
        }

        private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            ValidateClient(sessionConnectionContext);

            try
            {
                Validate.NotNull(sessionConnectionContext.IpAddress);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("The context is missing an IP address.", ex);
            }
        }

        private static void ValidateClient(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                Validate.NotNull(sessionConnectionContext.Client);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("The client must be set on the connection context before trying to negotiate a session reservation.", ex);
            }
        }

        private static async Task StartClientAsync(string ipAddress, IClient client, int port)
        {
            if (!client.IsConnected)
            {
                if (!NetworkInterface.GetAllNetworkInterfaces()
                    .Any(n => n.OperationalStatus == OperationalStatus.Up
                        && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                {
                    throw new ClientConnectionFailedException(Language.main.Get("Nitrox_NoConnection"));
                }

                await client.StartAsync(ipAddress, port);
                if (!client.IsConnected)
                {
                    if (client is LiteNetLibClient lnlClient && lnlClient.ConnectFailReason.HasValue)
                    {
                        throw new ClientConnectionFailedException(lnlClient.ConnectFailReason.Value);
                    }
                    throw new ClientConnectionFailedException(Language.main.Get("Nitrox_NoResponse"));
                }
            }
        }

        private static void EstablishSessionPolicy(IMultiplayerSessionConnectionContext sessionConnectionContext, IClient client)
        {
            string policyRequestCorrelationId = Guid.NewGuid().ToString();

            MultiplayerSessionPolicyRequest requestPacket = new MultiplayerSessionPolicyRequest(policyRequestCorrelationId);
            client.Send(requestPacket);

            EstablishingSessionPolicy nextState = new EstablishingSessionPolicy(policyRequestCorrelationId);
            sessionConnectionContext.UpdateConnectionState(nextState);
        }

        public void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("Cannot join a session until a reservation has been negotiated with the server.");
        }

        public void Disconnect(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("Not connected to a multiplayer server.");
        }
    }
}
