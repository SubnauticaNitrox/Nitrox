using System;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Helper;

namespace Nitrox.Client.Communication.MultiplayerSession.ConnectionState
{
    public class EstablishingSessionPolicy : ConnectionNegotiatingState
    {
        private string policyRequestCorrelationId;

        public EstablishingSessionPolicy(string policyRequestCorrelationId)
        {
            Validate.NotNull(policyRequestCorrelationId);
            this.policyRequestCorrelationId = policyRequestCorrelationId;
        }

        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY;

        public override void NegotiateReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                ValidateState(sessionConnectionContext);
                AwaitReservationCredentials(sessionConnectionContext);
            }
            catch (Exception)
            {
                Disconnect(sessionConnectionContext);
                throw;
            }
        }

        private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            SessionPolicyIsNotNull(sessionConnectionContext);
            SessionPolicyPacketCorrelation(sessionConnectionContext);
        }

        private static void SessionPolicyIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            try
            {
                Validate.NotNull(sessionConnectionContext.SessionPolicy);
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException("The context is missing a session policy.", ex);
            }
        }

        private void SessionPolicyPacketCorrelation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            Validate.PacketCorrelation(sessionConnectionContext.SessionPolicy, policyRequestCorrelationId);
        }

        private void AwaitReservationCredentials(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            AwaitingReservationCredentials nextState = new AwaitingReservationCredentials();
            sessionConnectionContext.UpdateConnectionState(nextState);
        }
    }
}
