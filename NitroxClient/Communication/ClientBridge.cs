using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxClient.Communication.Exceptions;

namespace NitroxClient.Communication
{
    //This is meant to be a drop-in replacement for the old PacketSender. It was re-written in accordance with the Bridge design pattern to clarify intent.
    //This pattern should give us greater flexibility in how we bridge the connection between the server and the client moving forward. 
    //Example: We can refactor this implementation into a ReservableClientBridge, allowing us to implement different types of client bridges depending on specific requirements.
    public class ClientBridge : IClientBridge
    {
        private IClient serverClient;
        private string correlationId;

        public ClientBridgeState CurrentState { get; private set; }
        public string ReservationKey { get; private set; }
        public ReservationRejectionReason ReservationRejectionReason { get; private set; }

        public ClientBridge(IClient serverClient)
        {
            this.serverClient = serverClient;
            correlationId = generateCorrelationId();

            CurrentState = ClientBridgeState.Disconnected;
            ReservationKey = null;
            ReservationRejectionReason = ReservationRejectionReason.None;
        }

        public void connect(string ipAddress, string playerName)
        {
            try
            {
                if (serverClient.IsConnected == false)
                {
                    serverClient.start(ipAddress);

                    if (serverClient.IsConnected == false)
                    {
                        throw new ClientConnectionFailedException("The client failed to indicate a Connected status without providing a reason for why it cannot reach the server.");
                    }
                }
            }
            catch (ClientConnectionFailedException ex)
            {
                tryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw ex;
            }
            catch (Exception ex)
            {
                tryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw new ClientConnectionFailedException("The connect operation failed for an unknown reason. Investigate the inner exception for additional details.", ex);
            }

            requestReservation(playerName);
        }

        public void disconnect()
        {
            tryStopClient();
            CurrentState = ClientBridgeState.Disconnected;
        }

        public void confirmReservation(string correlationId, string reservationKey)
        {
            try
            {
                if (CurrentState != ClientBridgeState.WaitingForRerservation)
                {
                    throw new InvalidReservationException();
                }

                validateCorrelationId(correlationId);
                validateReservationKey(reservationKey);
                confirmCorrelationId(correlationId);

                ReservationKey = reservationKey;
                ReservationRejectionReason = ReservationRejectionReason.None;

                CurrentState = ClientBridgeState.Connected;
            }
            catch (Exception ex)
            {
                tryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw ex;
            }
        }

        public void handleRejectedReservation(string correlationId, ReservationRejectionReason rejectionReason)
        {
            if (CurrentState == ClientBridgeState.Disconnected)
            {
                return;
            }

            try
            {
                validateCorrelationId(correlationId);
                confirmCorrelationId(correlationId);
                validateRejectionReason(rejectionReason);

                ReservationKey = null;
                ReservationRejectionReason = rejectionReason;

                CurrentState = ClientBridgeState.ReservationRejected;
            }
            catch (Exception ex)
            {
                tryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw ex;
            }
        }

        public void send(Packet packet)
        {
            if (CurrentState != ClientBridgeState.Connected)
            {
                throw new ProhibitedSendAttemptException();
            }

            serverClient.send(packet);
        }

        private void tryStopClient()
        {
            if (serverClient.IsConnected)
            {
                serverClient.stop();
            }
        }

        private void requestReservation(string playerName)
        {
            if (CurrentState == ClientBridgeState.WaitingForRerservation || CurrentState == ClientBridgeState.Connected)
            {
                throw new ProhibitedConnectAttemptException();
            }

            var reservePlayerSlot = new ReservePlayerSlot(correlationId, playerName);
            serverClient.send(reservePlayerSlot);

            CurrentState = ClientBridgeState.WaitingForRerservation;
        }

        private void validateCorrelationId(string correlationId)
        {
            if (correlationId == null)
            {
                throw new ParameterValidationException(nameof(correlationId), "The value cannot be null.");
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ParameterValidationException(nameof(correlationId), "The value cannot be blank.");
            }
        }

        private void validateReservationKey(string reservationKey)
        {
            if (reservationKey == null)
            {
                throw new ParameterValidationException(nameof(reservationKey), "The value cannot be null.");
            }

            if (string.IsNullOrEmpty(reservationKey))
            {
                throw new ParameterValidationException(nameof(reservationKey), "The value cannot be blank.");
            }
        }

        private void validateRejectionReason(ReservationRejectionReason rejectionReason)
        {
            if (rejectionReason == ReservationRejectionReason.None)
            {
                throw new InvalidReservationRejectionReasonException();
            }
        }

        private void confirmCorrelationId(string correlationId)
        {
            if (this.correlationId != correlationId)
            {
                throw new UncorrelatedMessageException();
            }
        }

        private string generateCorrelationId()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var seed = GetHashCode();
            var random = new Random(seed);

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}