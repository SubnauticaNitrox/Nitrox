using System;
using Nitrox.Model.Core;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Helper;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;

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

                RequestSessionReservation(sessionConnectionContext);
                AwaitSessionReservation(sessionConnectionContext, sessionConnectionContext.SessionPolicy.SessionId);
            }
            catch (Exception)
            {
                Disconnect(sessionConnectionContext);
                throw;
            }
            return Task.CompletedTask;
        }

        private void RequestSessionReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            IClient client = sessionConnectionContext.Client;
            PlayerSettings playerSettings = sessionConnectionContext.PlayerSettings;
            AuthenticationContext authenticationContext = sessionConnectionContext.AuthenticationContext;

            MultiplayerSessionReservationRequest requestPacket = new(playerSettings, authenticationContext);
            client.Send(requestPacket);
        }

        private void AwaitSessionReservation(IMultiplayerSessionConnectionContext sessionConnectionContext, SessionId reservationSessionId)
        {
            AwaitingSessionReservation nextState = new(reservationSessionId);
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
                throw new InvalidOperationException("The client is not connected.");
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
                throw new InvalidOperationException("The context does not contain player settings.", ex);
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
                throw new InvalidOperationException("The context does not contain an authentication context.", ex);
            }
        }
    }
}
