using System;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class AwaitingSessionReservation : ConnectionNegotiatingState
    {
        private string reservationCorrelationId;

        public AwaitingSessionReservation(string reservationCorrelationId)
        {
            Validate.NotNull(reservationCorrelationId);
            this.reservationCorrelationId = reservationCorrelationId;
        }

        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.AWAITING_SESSION_RESERVATION;

        public override void NegotiateReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                ValidateState(sessionConnectionContext);
                HandleReservation(sessionConnectionContext);
            }
            catch (Exception)
            {
                Disconnect(sessionConnectionContext);
                throw;
            }
        }

        private static void HandleReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            IMultiplayerSessionConnectionState nextState = sessionConnectionContext.Reservation.ReservationState == MultiplayerSessionReservationState.RESERVED
                ? new SessionReserved()
                : new SessionReservationRejected() as IMultiplayerSessionConnectionState;

            sessionConnectionContext.UpdateConnectionState(nextState);
        }

        private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            ReservationIsNotNull(sessionConnectionContext);
            ReservationPacketIsCorrelated(sessionConnectionContext);
        }

        private static void ReservationIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                Validate.NotNull(sessionConnectionContext.Reservation);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("The context does not have a reservation.", ex);
            }
        }

        private void ReservationPacketIsCorrelated(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            Validate.PacketCorrelation(sessionConnectionContext.Reservation, reservationCorrelationId);
        }
    }
}
