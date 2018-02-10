using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxClient.Communication.Exceptions;
using System.Collections.Generic;
using NitroxModel.PlayerSlot;
using NitroxModel.Logger;
using NitroxModel;
using NitroxModel.Packets.Exceptions;
using NitroxModel.Helper;

namespace NitroxClient.Communication
{
    //This is meant to be a drop-in replacement for the old PacketSender. It was re-written in accordance with the Bridge design pattern to clarify intent.
    //This pattern should give us greater flexibility in how we bridge the connection between the server and the client moving forward. 
    //Example: We can refactor this implementation into a ReservableClientBridge, allowing us to implement different types of client bridges depending on specific requirements.
    public class ClientBridge : IClientBridge
    {
        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();
        private string correlationId = Guid.NewGuid().ToString();
        private IClient serverClient;

        public ClientBridgeState CurrentState { get; private set; }
        public string ReservationKey { get; private set; }
        public PlayerSlotReservationState ReservationState { get; private set; }

        //Eww...
        public string PlayerId { get; private set; }

        public ClientBridge(IClient serverClient)
        {
            Log.Info("Initializing ClientBridge...");
            this.serverClient = serverClient;

            CurrentState = ClientBridgeState.Disconnected;
            ReservationKey = null;
            ReservationState = PlayerSlotReservationState.Rejected;
        }

        public void Connect(string ipAddress, string playerName)
        {
            try
            {
                if (serverClient.IsConnected == false)
                {
                    serverClient.Start(ipAddress);

                    if (serverClient.IsConnected == false)
                    {
                        throw new ClientConnectionFailedException("The client failed to indicate a Connected status without providing a reason for why it cannot reach the server.");
                    }
                }
            }
            catch (ClientConnectionFailedException ex)
            {
                TryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw ex;
            }
            catch (Exception ex)
            {
                TryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw new ClientConnectionFailedException("The connect operation failed for an unknown reason. Investigate the inner exception for additional details.", ex);
            }

            RequestReservation(playerName);
        }

        public void Disconnect()
        {
            TryStopClient();
            CurrentState = ClientBridgeState.Disconnected;
        }

        public void ConfirmReservation(string correlationId, string reservationKey)
        {
            try
            {
                if (CurrentState != ClientBridgeState.WaitingForRerservation)
                {
                    throw new InvalidReservationException();
                }

                ValidateCorrelationId(correlationId);
                ValidateReservationKey(reservationKey);
                ConfirmCorrelationId(correlationId);

                ReservationKey = reservationKey;
                ReservationState = PlayerSlotReservationState.Reserved;

                CurrentState = ClientBridgeState.Reserved;
            }
            catch (Exception ex)
            {
                TryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw ex;
            }
        }

        public void ClaimReservation()
        {
            try
            {
                if (CurrentState != ClientBridgeState.Reserved)
                {
                    throw new ProhibitedClaimAttemptException();
                }

                ClaimPlayerSlotReservation packet = new ClaimPlayerSlotReservation(correlationId, ReservationKey);
                serverClient.Send(packet);

                CurrentState = ClientBridgeState.Connected;
            }
            catch (Exception)
            {
                CurrentState = ClientBridgeState.Failed;
                throw;
            }
        }

        public void HandleRejectedReservation(string correlationId, PlayerSlotReservationState reservationState)
        {
            if (CurrentState == ClientBridgeState.Disconnected)
            {
                return;
            }

            try
            {
                if (CurrentState != ClientBridgeState.WaitingForRerservation)
                {
                    throw new ProhibitedReservationRejectionException();
                }

                ValidateCorrelationId(correlationId);
                ConfirmCorrelationId(correlationId);
                ValidateReservationState(reservationState);

                ReservationKey = null;
                ReservationState = reservationState;

                CurrentState = ClientBridgeState.ReservationRejected;
            }
            catch (Exception)
            {
                TryStopClient();
                CurrentState = ClientBridgeState.Failed;
                throw;
            }
        }

        public void Send(Packet packet)
        {
            if (CurrentState != ClientBridgeState.Connected)
            {
                throw new ProhibitedSendAttemptException();
            }

            if (!suppressedPacketsTypes.Contains(packet.GetType()))
            {
                serverClient.Send(packet);
            }
        }

        public PacketSuppression<T> Suppress<T>()
        {
            return new PacketSuppression<T>(suppressedPacketsTypes);
        }

        private void TryStopClient()
        {
            if (serverClient.IsConnected)
            {
                serverClient.Stop();
            }
        }

        private void RequestReservation(string playerName)
        {
            if (CurrentState == ClientBridgeState.WaitingForRerservation ||
                CurrentState == ClientBridgeState.Reserved ||
                CurrentState == ClientBridgeState.Connected)
            {
                throw new ProhibitedConnectAttemptException();
            }

            ReservePlayerSlot reservePlayerSlot = new ReservePlayerSlot(correlationId, playerName);
            serverClient.Send(reservePlayerSlot);

            PlayerId = playerName;
            CurrentState = ClientBridgeState.WaitingForRerservation;
        }

        private void ValidateCorrelationId(string correlationId)
        {
            Validate.NotNull(correlationId);

            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ParameterValidationException(nameof(correlationId), "The value cannot be blank.");
            }
        }

        private void ValidateReservationKey(string reservationKey)
        {
            Validate.NotNull(reservationKey);

            if (string.IsNullOrEmpty(reservationKey))
            {
                throw new ParameterValidationException(nameof(reservationKey), "The value cannot be blank.");
            }
        }

        private void ValidateReservationState(PlayerSlotReservationState reservationState)
        {
            if (reservationState == PlayerSlotReservationState.Reserved)
            {
                throw new InvalidReservationRejectionReasonException();
            }
        }

        private void ConfirmCorrelationId(string correlationId)
        {
            if (this.correlationId != correlationId)
            {
                throw new UncorrelatedMessageException();
            }
        }
    }
}
