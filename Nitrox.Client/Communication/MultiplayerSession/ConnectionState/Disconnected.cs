using System;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Exceptions;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.MultiplayerSession.ConnectionState
{
    public class Disconnected : IMultiplayerSessionConnectionState
    {
        private object stateLock = new object();
        public MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.DISCONNECTED;

        public void NegotiateReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            lock (stateLock)
            {
                ValidateState(sessionConnectionContext);

                IClient client = sessionConnectionContext.Client;
                string ipAddress = sessionConnectionContext.IpAddress;
                int port = sessionConnectionContext.ServerPort;
                StartClient(ipAddress, client,port);
                EstablishSessionPolicy(sessionConnectionContext, client);
            }
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

        private static void StartClient(string ipAddress, IClient client, int port)
        {
            if (!client.IsConnected)
            {
                client.Start(ipAddress,port);

                if (!client.IsConnected)
                {
                    throw new ClientConnectionFailedException("The client failed to connect without providing a reason why.");
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
