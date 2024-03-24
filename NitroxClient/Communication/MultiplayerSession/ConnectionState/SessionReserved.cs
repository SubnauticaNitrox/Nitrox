using System;
using NitroxClient.Communication.Abstract;
using NitroxModel;
using NitroxModel.Packets;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class SessionReserved : ConnectionNegotiatedState
    {
        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.SESSION_RESERVED;

        public override void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                ValidateState(sessionConnectionContext);
                EnterMultiplayerSession(sessionConnectionContext);
                ChangeState(sessionConnectionContext);
            }
            catch (Exception)
            {
                Disconnect(sessionConnectionContext);
            }
        }

        private static void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            if (!sessionConnectionContext.Client.IsConnected)
            {
                DisplayStatusCode(StatusCode.CONNECTION_FAIL_CLIENT, "The client is not connected.");
            }
        }

        private void EnterMultiplayerSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            IClient client = sessionConnectionContext.Client;
            MultiplayerSessionReservation reservation = sessionConnectionContext.Reservation;
            string correlationId = reservation.CorrelationId;
            string reservationKey = reservation.ReservationKey;

            PlayerJoiningMultiplayerSession packet = new PlayerJoiningMultiplayerSession(correlationId, reservationKey);
            client.Send(packet);
        }

        private void ChangeState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            SessionJoined nextState = new SessionJoined();
            sessionConnectionContext.UpdateConnectionState(nextState);
        }
    }
}
