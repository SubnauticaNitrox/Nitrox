using System;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets.Exceptions;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class AwaitingSessionReservation : ConnectionNegotiatingState
    {
        private readonly string reservationCorrelationId;

        public AwaitingSessionReservation(string reservationCorrelationId)
        {
            Validate.NotNull(reservationCorrelationId);
            this.reservationCorrelationId = reservationCorrelationId;
        }

        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.AWAITING_SESSION_RESERVATION;

        public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
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
            return Task.CompletedTask;
        }

        private static void HandleReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            IMultiplayerSessionConnectionState nextState = sessionConnectionContext.Reservation.ReservationState switch
            {
                MultiplayerSessionReservationState.RESERVED => new SessionReserved(),
                _ => new SessionReservationRejected(),
            };

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
                DisplayStatusCode(StatusCode.invalidPacket, false, "The context does not have a reservation." + ex.ToString());
            }
        }

        private void ReservationPacketIsCorrelated(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            if (!reservationCorrelationId.Equals(sessionConnectionContext.Reservation.CorrelationId))
            {
                DisplayStatusCode(StatusCode.invalidPacket, false, sessionConnectionContext.Reservation.ToString() + reservationCorrelationId.ToString());
                throw new UncorrelatedPacketException(sessionConnectionContext.Reservation, reservationCorrelationId);
            }
        }
    }
}
