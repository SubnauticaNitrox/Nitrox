using System;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets.Exceptions;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class EstablishingSessionPolicy : ConnectionNegotiatingState
    {
        private readonly string policyRequestCorrelationId;

        public EstablishingSessionPolicy(string policyRequestCorrelationId)
        {
            Validate.NotNull(policyRequestCorrelationId);
            this.policyRequestCorrelationId = policyRequestCorrelationId;
        }

        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY;

        public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
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
            return Task.CompletedTask;
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
                DisplayStatusCode(StatusCode.invalidPacket, false, "The context is missing a session policy." + ex.ToString());
            }
        }

        private void SessionPolicyPacketCorrelation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            if (!policyRequestCorrelationId.Equals(sessionConnectionContext.SessionPolicy.CorrelationId))
            {
                DisplayStatusCode(StatusCode.invalidPacket, false, "The packet was uncorrelated to the function of this processor: ");
            }
        }

        private void AwaitReservationCredentials(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            AwaitingReservationCredentials nextState = new AwaitingReservationCredentials();
            sessionConnectionContext.UpdateConnectionState(nextState);
        }
    }
}
