using System;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class AwaitingReservationCredentials : ConnectionNegotiatingState
    {
        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS;

        public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                ValidateState(sessionConnectionContext);

                string reservationCorrelationId = Guid.NewGuid().ToString();
                RequestSessionReservation(sessionConnectionContext, reservationCorrelationId);
                AwaitSessionReservation(sessionConnectionContext, reservationCorrelationId);
            }
            catch (Exception e)
            {
                Disconnect(sessionConnectionContext);
            }
            return Task.CompletedTask;
        }

        private void RequestSessionReservation(IMultiplayerSessionConnectionContext sessionConnectionContext, string reservationCorrelationId)
        {
            IClient client = sessionConnectionContext.Client;
            PlayerSettings playerSettings = sessionConnectionContext.PlayerSettings;
            AuthenticationContext authenticationContext = sessionConnectionContext.AuthenticationContext;

            MultiplayerSessionReservationRequest requestPacket = new(reservationCorrelationId, playerSettings, authenticationContext);
            client.Send(requestPacket);
        }

        private void AwaitSessionReservation(IMultiplayerSessionConnectionContext sessionConnectionContext, string reservationCorrelationId)
        {
            AwaitingSessionReservation nextState = new AwaitingSessionReservation(reservationCorrelationId);
            sessionConnectionContext.UpdateConnectionState(nextState);
        }

        private static void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            ClientIsConnected(sessionConnectionContext);
            PlayerSettingsIsNotNull(sessionConnectionContext);
            AuthenticationContextIsNotNull(sessionConnectionContext);
        }

        private static void ClientIsConnected(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            if (!sessionConnectionContext.Client.IsConnected)
            {
                DisplayStatusCode(StatusCode.INVALID_PACKET, "The client is not connected.");
            }
        }

        private static void PlayerSettingsIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                Validate.NotNull(sessionConnectionContext.PlayerSettings);
            }
            catch (ArgumentNullException ex)
            {
                DisplayStatusCode(StatusCode.INVALID_PACKET, "The context does not contain player settings." + ex);
            }
        }

        private static void AuthenticationContextIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                Validate.NotNull(sessionConnectionContext.AuthenticationContext);
            }
            catch (ArgumentNullException ex)
            {
                DisplayStatusCode(StatusCode.INVALID_PACKET, "The context does not contain an authentication context." + ex);
            }
        }
    }
}
