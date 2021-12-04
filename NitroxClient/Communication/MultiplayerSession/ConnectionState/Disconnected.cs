using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Networking;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
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
                IConnectionInfo connectionInfo = sessionConnectionContext.ConnectionInfo;
                StartClient(connectionInfo, client);
                EstablishSessionPolicy(sessionConnectionContext, client);
            }
        }

        private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            ValidateClient(sessionConnectionContext);

            try
            {
                Validate.NotNull(sessionConnectionContext.ConnectionInfo);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("The context is missing the connection info.", ex);
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

        private static void StartClient(IConnectionInfo connectionInfo, IClient client)
        {
            if (!client.IsConnected)
            {
                client.Start(connectionInfo);

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
